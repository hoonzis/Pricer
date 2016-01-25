﻿namespace OptionsPricing

open System
open MathNet.Numerics.Distributions


type OptionKind = 
        | Call
        | Put

type OptionStyle = 
    | American
    | European

type Implementation =
    | Functional 
    | Imperative

type OptionLeg = 
    {
        Direction : float
        Strike : float
        Expiry : DateTime
        Kind : OptionKind
        Style: OptionStyle 
        PurchaseDate: DateTime
    }
    member this.BuyVsSell = if this.Direction < 0.0 then "Sell" else "Buy"
    member this.TimeToExpiry = (float (this.Expiry - this.PurchaseDate).Days)/365.0
    member this.Name = sprintf "%s %s %f" this.BuyVsSell (Tools.caseString this.Kind) this.Strike

type CashLeg = 
    {
        Direction: float
        Strike:float
    }
    
type LegInfo = 
    | Cash of CashLeg
    | Option of OptionLeg
    member this.Name = match this with
                            | Cash cl -> "Cash"
                            | Option ol -> ol.Name
type Pricing = 
    {   Delta: float
        Premium: float
    }

type Leg = {
    Definition:LegInfo
    Pricing:Pricing option
}

type Strategy = {
        Stock : StockInfo
        Name : String
        Legs: Leg list
    }

type LegData = {
    Leg:Leg
    LegData: float*float list
}

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

module Options = 
    
    let normal = Normal()

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
            |> Seq.fold (fun prices' ((sDown,dDown),(sUp,dUp)) -> 
                let derValue = (pricing.PUp*dUp+pricing.PDown*dDown)*(1.0/pricing.Rate)
                let stockValue = sUp*pricing.Down
                let der' = if pricing.Option.Style = American then
                                let prematureExValue = optionVal stockValue
                                max derValue prematureExValue
                            else derValue
                prices' @ [stockValue,der'])[]

    let binomialPricingFunc (pricing:BinomialPricing) =
        let optionVal stock = 
            match pricing.Option.Kind with
                    | Call -> max 0.0 (stock - pricing.Option.Strike)
                    | Put -> max 0.0 (pricing.Option.Strike - stock)
        
        let prices = generateEndNodePrices pricing.Ref pricing.Up pricing.Periods optionVal
        
        let reductionStep = step pricing optionVal
        let rec reducePrices prices =
            match prices with
                    | [(stock,der)] -> der
                    | prs -> reducePrices (reductionStep prs)
        
        let premium = reducePrices prices
        {
            Premium = premium
            Delta = 1.0
        }

    let buildLeg kind strike direction style expiry = 
        {
            Strike=strike
            Kind = Call
            Direction = direction
            Expiry = expiry
            Style = European
            PurchaseDate = DateTime.Now
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

    let blackScholes (stock:StockInfo) (option:OptionLeg) =
        let price,delta = 
            // We can only calculate if the option concerns the future
            if option.TimeToExpiry > 0.0 then
                // Calculate d1 and d2 and pass them to cumulative distribution
                let d1 = 
                    ( log(stock.CurrentPrice / option.Strike) + 
                        (stock.Rate + 0.5 * pown stock.Volatility 2) * option.TimeToExpiry ) /
                    ( stock.Volatility * sqrt option.TimeToExpiry )
                let d2 = d1 - stock.Volatility * sqrt option.TimeToExpiry
                let N1 = normal.CumulativeDistribution(d1)
                let N2 = normal.CumulativeDistribution(d2)

                let discountedStrike = option.Strike * exp (-stock.Rate * option.TimeToExpiry)
                let call = stock.CurrentPrice * N1 - discountedStrike * N2
                match option.Kind with
                    | Call -> call, N1
                    | Put -> call + discountedStrike - stock.CurrentPrice, N1 - 1.0
            else
                // If the option has expired, calculate payoff directly
                match option.Kind with
                    | Call -> max (stock.CurrentPrice - option.Strike) 0.0,1.0
                    | Put -> max (option.Strike - stock.CurrentPrice) 0.0,1.0
        {
            Premium = price
            Delta = delta
        }

    let europeanBSPrice (rate:decimal) (direction:float) (ref:decimal) (vol:decimal) (strike:decimal) (expiry:DateTime) (legType:OptionKind) = 
        let leg = buildLeg legType (float strike) direction European expiry
        let stockInfo = { 
            Rate = float rate
            Volatility = float vol
            CurrentPrice = float ref
        }
        blackScholes stockInfo leg

    let addPricing option pricing = 
        {
            Definition = option
            Pricing = Some pricing
        }
    
    let optionPayoff option ref premium = 
        match option.Kind with
                | Call -> (max (ref - option.Strike) 0.0) - premium
                | Put -> (max (option.Strike - ref) 0.0) - premium

    let cashPayoff cash ref =
        (ref - cash.Strike)

    let getStrategyData (strategy:Strategy) = 
        let strikes = strategy.Legs |> Seq.map (fun s -> 
            match s.Definition with
                | Cash cl -> cl.Strike
                | Option ol -> ol.Strike
        )
        let min = 0.5*(strikes |> Seq.min)
        let max = 1.5*(strikes |> Seq.max)

        //either pricing is provied for each leg or we have to calculate it
        //for each leg get pricedLeg and a function that calculates it's payoff
        let payOffs = strategy.Legs |> Seq.map (fun leg ->
            let pricedLeg = 
                match leg.Pricing with
                        | Some pricing -> pricing
                        | None -> 
                            match leg.Definition with
                                    | Cash cl -> 
                                        {
                                            Premium = cl.Strike
                                            Delta = 1.0
                                        }
                                       
                                    | Option ol -> blackScholes strategy.Stock ol
            let payoffCalculator = 
                match leg.Definition with
                    | Cash cl -> (fun ref -> cl.Direction * cashPayoff cl ref)
                    | Option ol -> (fun ref -> ol.Direction * (optionPayoff ol ref pricedLeg.Premium))
            (addPricing leg.Definition pricedLeg), payoffCalculator
        )
        //only some x points are interesting - precisely all the strikes
        let interestingPoints = ([min] @ (strikes |> List.ofSeq) @ [max]) |> List.sort
        let strategyData = [for ref in interestingPoints -> ref, payOffs |> Seq.sumBy (fun (leg,payOff) -> payOff ref)]
        let legsData = payOffs |> Seq.map (fun (leg,payOff) -> leg,[for ref in interestingPoints -> ref, payOff ref])
        (strategyData, legsData)