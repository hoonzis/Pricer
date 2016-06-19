namespace Pricer.Core

open System

type PayoffsGenerator (pricer:IPricer) =

    member this.legPricing stock leg =
        match leg.Definition with
            | Cash cashLeg -> pricer.priceCash cashLeg
            | Option optionLeg -> pricer.priceOption stock optionLeg
            | Convertible convertible -> pricer.priceConvert stock convertible

    //only some x points are interesting - precisely all the strikes
    member this.getInterestingPoints strategy =
        if strategy.Legs |> Seq.isEmpty then Seq.empty
        else
            let strikes = strategy.Legs |> List.map (fun leg ->
                match leg.Definition with
                    | Cash cash -> cash.Price
                    | Option option -> option.Strike
                    | Convertible convertible -> convertible.ReferencePrice 
            )
            let min = 0.5*(strikes |> Seq.min)
            let max = 1.5*(strikes |> Seq.max)
            seq {
                yield min
                yield! (strikes |> Seq.sort)
                yield max
            }
    member this.legPayoff leg pricing (year:int) stockPrice =
        match leg with

            | Cash cashLeg -> cashLeg.Direction * (stockPrice - cashLeg.Price)
            | Option optionLeg -> optionLeg.Direction * (BasicOptions.optionValue optionLeg stockPrice - pricing.Premium)
            | Convertible convertible -> convertible.Direction * (float year * convertible.Coupon * convertible.FaceValue - pricing.Premium)


    member this.getStrategyData (strategy:Strategy) =
        let getLegPricing leg =
            match leg.Pricing with
                | Some pricing -> pricing
                | None -> this.legPricing strategy.Stock leg

        
        let payOffsPerLeg = strategy.Legs |> Seq.map (fun leg ->
            let pricing = getLegPricing leg
            let pricedLeg = { leg with Pricing = Some pricing }
            let payoffCalculator = this.legPayoff leg.Definition pricing   
            pricedLeg, payoffCalculator
        )

        let interestingPoints = this.getInterestingPoints strategy
        let hasConverts = strategy.Legs |> Seq.exists (fun leg -> match leg.Definition with | Convertible _ -> true | _ -> false)
        
        match hasConverts with
            | true -> 
                let years = [1;2;3]

                let legsData = payOffsPerLeg |> Seq.map (fun (leg,payOff) -> 
                    leg, years |> Seq.map (fun year -> [for stockPrice in interestingPoints  do yield stockPrice, payOff year stockPrice])
                )

                let strategyData = years |> Seq.map (fun year -> 
                    [for stockPrice in interestingPoints do yield stockPrice, payOffsPerLeg |> Seq.sumBy (fun (leg,payOff) -> payOff year stockPrice)]
                )

                MultiYear strategyData
            | false -> 
                let legsData = payOffsPerLeg |> Seq.map (fun (leg,payOff) -> 
                    leg, [for stockPrice in interestingPoints  do yield stockPrice, payOff 1 stockPrice]
                )

                let strategyData = [for stockPrice in interestingPoints do yield stockPrice, payOffsPerLeg |> Seq.sumBy (fun (leg,payOff) -> payOff 1 stockPrice)]
                
                SingleYear (strategyData,legsData)