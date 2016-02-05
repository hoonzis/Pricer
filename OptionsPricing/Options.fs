namespace OptionsPricing

open System
open MathNet.Numerics.Distributions

type OptionKind = 
    | Call
    | Put

type OptionStyle = 
    | American
    | European


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
    member this.Name = sprintf "%s %s %.2f" this.BuyVsSell (Tools.caseString this.Kind) this.Strike

type CashLeg = {
    Direction: float
    Strike:float
}
    
type LegInfo = 
    | Cash of CashLeg
    | Option of OptionLeg
    member this.Name = match this with
                            | Cash cl -> "Cash"
                            | Option ol -> ol.Name
type Pricing = {
    Delta: float
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




module Options = 
    
    let normal = Normal()

    let buildLeg kind strike direction style expiry = 
        {
            Strike=strike
            Kind = Call
            Direction = direction
            Expiry = expiry
            Style = European
            PurchaseDate = DateTime.Now
        }

    let optionValue option ref = 
        match option.Kind with
                | Call -> max 0.0 (ref - option.Strike)
                | Put -> max 0.0 (option.Strike - ref)
    
    let legPayoff leg pricing ref =
        match leg with
            | Cash cashLeg -> (ref - cashLeg.Strike)
            | Option optionLeg -> optionValue optionLeg ref - pricing.Premium
        
    
    let cashPricing (leg:CashLeg) = {
        Premium = leg.Strike
        Delta = 1.0
    }

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

    

    let getLegPricing stock leg =
        match leg.Definition with
            | Cash cashLeg -> cashPricing cashLeg
            | Option optionLeg -> blackScholes stock optionLeg

    //only some x points are interesting - precisely all the strikes
    let getInterestingPoints strategy =
        if strategy.Legs |> Seq.isEmpty then []
        else
            let strikes = strategy.Legs |> List.map (fun s -> 
                match s.Definition with
                    | Cash cl -> cl.Strike
                    | Option ol -> ol.Strike
            )
            let min = 0.5*(strikes |> Seq.min)
            let max = 1.5*(strikes |> Seq.max)
            ([min] @ strikes @ [max]) |> List.sort

    let getStrategyData (strategy:Strategy) = 
        let payOffs = strategy.Legs |> Seq.map (fun leg ->
            let legPricing = 
                match leg.Pricing with
                        | Some pricing -> pricing
                        | None -> getLegPricing strategy.Stock leg
            
            let pricedLeg = { leg with Pricing = Some legPricing }
            let payoffCalculator = legPayoff pricedLeg.Definition legPricing
            pricedLeg, payoffCalculator
        )
        
        let interestingPoints = getInterestingPoints strategy
        let strategyData = [for ref in interestingPoints -> ref, payOffs |> Seq.sumBy (fun (leg,payOff) -> payOff ref)]
        let legsData = payOffs |> Seq.map (fun (leg,payOff) -> leg,[for ref in interestingPoints -> ref, payOff ref])
        (strategyData, legsData)