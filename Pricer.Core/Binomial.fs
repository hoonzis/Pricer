namespace Pricer.Core

type Implementation =
    | Functional 
    | Imperative


//this type holds the configuration for binomial pricing model
type BinomialPricing = {
    Periods : int
    Down : float
    Up :float
    PUp:float
    PDown:float
    Option: OptionLeg
    Rate:float
    Ref:float
}

type BinomialNode = {
    Stock: double
    Option: double
    PreviousOption: double
}

module Binomial =
    
    let binomialPrice (ref:float) (strike:float) (rate:float) (up:float) =
        let down = 1.0/up
        let q = (exp (-rate)-down) / (up - down) 
        let cu = max 0.0 (up*ref-strike)
        let cd = max 0.0 (down*ref-strike)
        let call = exp(-rate) * (q*cu + (1.0-q)*cd)
        call

    let buildPricingResult previousOptionPrice optionPrice pricing = 
        let optionPriceChange = previousOptionPrice - optionPrice
        let underlyingPriceChange = pricing.Ref*pricing.Up - pricing.Ref
        let delta = optionPriceChange / underlyingPriceChange
        {
            Premium = optionPrice
            Delta = delta
        }

    let binomialPricing (pricing:BinomialPricing) =
        
        // this array holds the prices of the underlying in given period
        let prices = Array.zeroCreate pricing.Periods
        
        // retursn a function used to calculate the option prices in the period i.
        // it looks into the underlying prices array and compares with strike
        let optionValueInPeriod = 
            match pricing.Option.Kind with
                    | Call -> fun i -> max 0.0 (prices.[i] - pricing.Option.Strike)
                    | Put -> fun i -> max 0.0 (pricing.Option.Strike - prices.[i])
                                 
        // initialize the last price (the underlying wen down * period times)
        prices.[0] <- pricing.Ref*(pricing.Down**(float pricing.Periods))
        let optionValues = Array.zeroCreate pricing.Periods

        optionValues.[0]<- optionValueInPeriod 0
        for i in 1 ..pricing.Periods-1 do
            prices.[i] <- prices.[i-1]*pricing.Up*pricing.Up
            optionValues.[i]<- optionValueInPeriod i

        let counter = pricing.Periods-2
        for step = counter downto 0 do
            for j in 0 .. step do
                optionValues.[j] <- (pricing.PUp*optionValues.[j+1]+pricing.PDown*optionValues.[j])*(1.0/pricing.Rate)
                if pricing.Option.Style = American then
                    prices.[j] <- pricing.Down*prices.[j+1]
                    optionValues.[j] <- max optionValues.[j] (optionValueInPeriod j)

        buildPricingResult optionValues.[1] optionValues.[0] pricing
    
    // Creates an array of BinomialNodes - that leafs of the CRR tree
    let generateEndNodePrices (ref:float) (up:float) (periods:int) optionVal =
        let down = 1.0 / up 
        let lowestStock = ref*(down**(float periods))
        let first = {
            Stock = lowestStock
            Option = optionVal lowestStock
            PreviousOption = 0.0
        }
        let values = Seq.unfold (fun node -> 
            let stock' = node.Stock*up*up
            let option' = optionVal stock'
            let nodeBellow = {
                Stock = stock'
                Option = option'
                PreviousOption = 0.0
            }
            Some (node,nodeBellow)) first
        values |> Seq.take periods |> Seq.toArray


    // merge two nodes
    let mergeNodes downNode upNode optionVal pricing = 
        // calculate the value of the next option, using the CRR
        let derValue = (pricing.PUp * upNode.Option + pricing.PDown * downNode.Option)*(1.0/pricing.Rate)

        // calculate the value of the next stock - simply by raising the old stock
        let stockValue = upNode.Stock * pricing.Down

        // check for premature execution - if it's american option
        let option' = 
            match pricing.Option.Style with
                | American -> 
                    let prematureExValue = optionVal stockValue
                    max derValue prematureExValue
                | European -> derValue


        {
            Stock = stockValue
            Option = option'
            PreviousOption = upNode.Option
        }

    // takes one layer of the binomial tree and generates the next layer
    let step pricing optionVal (prices:BinomialNode []) =
        prices 
            |> Array.pairwise 
            |> Array.map (fun (downNode,upNode) -> mergeNodes downNode upNode optionVal pricing)
            
    let binomialPricingFunc (pricing:BinomialPricing) =
        let optionValue = BasicOptions.optionValue pricing.Option

        // that's the leafs of the derivative tree
        let prices = generateEndNodePrices pricing.Ref pricing.Up pricing.Periods optionValue
        
        // define a reduction step which will generate new layer
        let reductionStep = step pricing optionValue
        
        // apply the reduction till we have only one node in the list
        let rec reducePrices prices =
            match prices with
                    | [|node|] -> node.Option, node.PreviousOption
                    | prs -> reducePrices (reductionStep prs)

        let premium, previousPremium = reducePrices prices
        buildPricingResult previousPremium premium pricing

    let binomial (stock:StockInfo) (option:OptionLeg) (steps:int) implementation = 

        // we need to construct the binomial pricing model, using the CRR (Cox, Ross and Rubinstein)
        // the original model is composed of 3 parameters p,u,d. 
        // u - up probability, d - down probability. p is the technical probability
        // here we have PUp and PDown, for further simplifacation of the calculations
        let deltaT = option.TimeToExpiry/float steps
        let up = exp(stock.Volatility*sqrt deltaT)
        let down = 1.0/up
        let R = exp (stock.Rate*deltaT)
        let p_up = (R-down)/(up-down)
        let p_down = 1.0 - p_up

        let pricing = {
            Periods = steps
            Up = up
            Down = down
            PUp = p_up
            PDown = p_down
            Rate = R
            Option = option
            Ref = stock.CurrentPrice
        }
        
        match implementation with
            | Imperative -> binomialPricing pricing
            | Functional -> binomialPricingFunc pricing