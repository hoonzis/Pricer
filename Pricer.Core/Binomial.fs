namespace Pricer.Core

type Implementation =
    | Functional 
    | Imperative


// this type holds the configuration for binomial pricing model
type BinomialContext = {
    Periods : int
    Down : float
    Up :float
    PUp:float
    PDown:float
    Option: OptionLeg
    Rate:float
    Ref:float
}

// Single node in the CRR pricing tree
// Holds the stock pricer in given node, the options price
// and also the option price in the previous step
// we need to keep the track of the option pricer change, since that information is used
// to calculate the delta
type BinomialNode = {
    Stock: double
    Option: double
    UpParent: BinomialNode option
    DownParent: BinomialNode option
}


module Binomial =
    
    let emptyNode = {
        Stock = 0.0
        Option = 0.0
        UpParent = None
        DownParent = None
    }
    
    let binomialPrice (ref:float) (strike:float) (rate:float) (up:float) =
        let down = 1.0/up
        let q = (exp (-rate)-down) / (up - down) 
        let cu = max 0.0 (up*ref-strike)
        let cd = max 0.0 (down*ref-strike)
        let call = exp(-rate) * (q*cu + (1.0-q)*cd)
        call

    let buildPricingResult previousOptionPrice optionPrice ctx = 
        let optionPriceChange = previousOptionPrice - optionPrice
        let underlyingPriceChange = ctx.Ref*ctx.Up - ctx.Ref
        let delta = optionPriceChange / underlyingPriceChange
        {
            Premium = optionPrice
            Delta = delta
        }

    let binomialPricing (ctx: BinomialContext) =
        
        // this array holds the prices of the underlying in given period
        let prices = Array.zeroCreate ctx.Periods
        let option = ctx.Option
        
        // returns a function used to calculate the option prices in the period i.
        // it looks into the underlying prices array and compares with strike
        let optionValueInPeriod = 
            match option.Kind with
                    | Call -> fun i -> max 0.0 (prices.[i] - option.Strike)
                    | Put -> fun i -> max 0.0 (option.Strike - prices.[i])
                                 
        // initialize the last price (the underlying wen down * period times)
        prices.[0] <- ctx.Ref*(ctx.Down**(float ctx.Periods))
        let optionValues = Array.zeroCreate ctx.Periods

        optionValues.[0]<- optionValueInPeriod 0
        for i in 1 ..ctx.Periods-1 do
            prices.[i] <- prices.[i-1]*ctx.Up*ctx.Up
            optionValues.[i]<- optionValueInPeriod i

        let counter = ctx.Periods-2
        for step = counter downto 0 do
            for j in 0 .. step do
                optionValues.[j] <- (ctx.PUp*optionValues.[j+1]+ctx.PDown*optionValues.[j])*(1.0/ctx.Rate)
                if option.Style = American then
                    prices.[j] <- ctx.Down*prices.[j+1]
                    optionValues.[j] <- max optionValues.[j] (optionValueInPeriod j)

        buildPricingResult optionValues.[1] optionValues.[0] ctx
    
    // Creates an array of BinomialNodes - the leafs of the CRR tree
    let generateEndNodePrices (ref:float) (up:float) (periods:int) optionVal =
        let down = 1.0 / up 
        let lowestStock = ref*(down**(float periods))
        let first = { emptyNode with Stock = lowestStock; Option = optionVal lowestStock}
        let values = Seq.unfold (fun node -> 
            let stock' = node.Stock*up*up
            let option' = optionVal stock'
            let nodeBellow = { 
                emptyNode with
                    Stock = stock'
                    Option = option'
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
            UpParent = Some upNode
            DownParent = Some downNode
        }

    // takes one layer of the binomial tree and generates the next layer
    let step pricing optionVal (prices:BinomialNode []) =
        prices 
            |> Array.pairwise 
            |> Array.map (fun (downNode,upNode) -> mergeNodes downNode upNode optionVal pricing)
            
    let binomialPricingFunc (ctx:BinomialContext) =
        let optionValue = BasicOptions.optionValue ctx.Option

        // that's the leafs of the derivative tree
        let prices = generateEndNodePrices ctx.Ref ctx.Up ctx.Periods optionValue
        
        // define a reduction step which will generate new layer
        let reductionStep = step ctx optionValue
        
        // apply the reduction till we have only one node in the list
        let rec reducePrices prices =
            match prices with
                    | [|node|] -> node.Option, node.UpParent.Value.Option
                    | prs -> reducePrices (reductionStep prs)

        let premium, previousPremium = reducePrices prices
        buildPricingResult previousPremium premium ctx

    let buildPricingContext (stock:StockInfo) (option:OptionLeg) (steps:int) = 
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

        {
            Periods = steps
            Up = up
            Down = down
            PUp = p_up
            PDown = p_down
            Rate = R
            Option = option
            Ref = stock.CurrentPrice
        }

    let binomial (stock:StockInfo) (option:OptionLeg) (steps:int) implementation = 
        let context = buildPricingContext stock option steps
        match implementation with
            | Imperative -> binomialPricing context
            | Functional -> binomialPricingFunc context