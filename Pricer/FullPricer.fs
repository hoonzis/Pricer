namespace Pricer

open System
open Pricer.Core

type FullPricer()= 
    
    let bsPricer = new BlackScholesPricer(new MathNetProvider())

    interface IPricer with
        member this.priceOption stock option = bsPricer.blackScholes stock option

        member this.priceCash cash = {
            Premium = cash.Price
            Delta = 1.0
        }

        member this.priceConvert stock option = failwith "implement CB pricing"

    
    member this.europeanPrice (rate:decimal) (direction:float) (ref:decimal) (vol:decimal) (strike:decimal) (expiry:DateTime) (legType:OptionKind) (buyingDate: DateTime) =
        let leg = BasicOptions.buildLeg legType (float strike) direction European expiry buyingDate
        let stockInfo = {
            Rate = float rate
            Volatility = float vol
            CurrentPrice = float ref
        }
        bsPricer.blackScholes stockInfo leg

