namespace Pricer

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

module Binomial =
    
    let binomialPrice (ref:float) (strike:float) (rate:float) (up:float) =
        let down = 1.0/up
        let q = (exp (-rate)-down) / (up - down) 
        let cu = max 0.0 (up*ref-strike)
        let cd = max 0.0 (down*ref-strike)
        let call = exp(-rate) * (q*cu + (1.0-q)*cd)
        call

    let binomialPricing (pricing:BinomialPricing) =
        let prices = Array.zeroCreate pricing.Periods
        let optionValue = 
            match pricing.Option.Kind with
                    | Call -> fun i -> max 0.0 (prices.[i] - pricing.Option.Strike)
                    | Put -> fun i -> max 0.0 (pricing.Option.Strike - prices.[i])
                                 
        prices.[0] <- pricing.Ref*(pricing.Down**(float pricing.Periods))
        let oValues = Array.zeroCreate pricing.Periods
        oValues.[0]<- optionValue 0
        for i in 1 ..pricing.Periods-1 do
            prices.[i] <- prices.[i-1]*pricing.Up*pricing.Up
            oValues.[i]<- optionValue i

        let counter = pricing.Periods-2
        for step = counter downto 0 do
            for j in 0 .. step do
                oValues.[j] <- (pricing.PUp*oValues.[j+1]+pricing.PDown*oValues.[j])*(1.0/pricing.Rate)
                if pricing.Option.Style = American then
                    prices.[j] <- pricing.Down*prices.[j+1]
                    oValues.[j] <- max oValues.[j] (optionValue j)
        let delta = (oValues.[1] - oValues.[1]) / (pricing.Ref*pricing.Up - pricing.Ref*pricing.Down)
        {
            Premium = oValues.[0]
            Delta = delta
        }

    
    let generateEndNodePrices (ref:float) (up:float) (periods:int) optionVal =
        let down = 1.0 / up 
        let lowestStock = ref*(down**(float periods))
        let first = lowestStock,optionVal lowestStock
        let values = Seq.unfold (fun (stock,der)-> 
            let stock' = stock*up*up
            let der' = optionVal stock'
            Some ((stock,der),(stock', der'))) first
        values |> Seq.take periods |> List.ofSeq

    let step pricing optionVal (prices:(float*float) list) =
        prices 
            |> Seq.pairwise 
            |> Seq.map (fun ((sDown,dDown),(sUp,dUp)) -> 
                let derValue = (pricing.PUp*dUp+pricing.PDown*dDown)*(1.0/pricing.Rate)
                let stockValue = sUp*pricing.Down
                let der' = match pricing.Option.Style with
                                    | American -> 
                                        let prematureExValue = optionVal stockValue
                                        max derValue prematureExValue
                                    | European -> derValue
                stockValue,der')
            |> List.ofSeq

    let binomialPricingFunc (pricing:BinomialPricing) =
        let optionValue = BasicOptions.optionValue pricing.Option
        let prices = generateEndNodePrices pricing.Ref pricing.Up pricing.Periods optionValue
        
        let reductionStep = step pricing optionValue
        let rec reducePrices prices =
            match prices with
                    | [(stock,der)] -> der
                    | prs -> reducePrices (reductionStep prs)
        //TODO: calculate the correct delta in this functional implementation    
        let premium = reducePrices prices
        {
            Premium = premium
            Delta = 1.0
        }

    let binomial (stock:StockInfo) (option:OptionLeg) (steps:int) implementation = 

        //we need to construct the binomial pricing model, using the CRR (Cox, Ross and Rubinstein)
        //the original model is composed of 3 parameters p,u,d. 
        //u - up probability, d - down probability. p is the technical probability
        //here we have PUp and PDown, for further simplifacation of the calculations
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