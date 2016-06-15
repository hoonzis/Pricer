namespace Pricer.Full

open System
open Pricer

type FullPricer()= 
    
    let bsPricer = new BsPricer(new MathNetProvider())

    interface IPricer with
        member this.priceOption stock option = bsPricer.blackScholes stock option

        member this.priceCash cash = bsPricer.cashPricing cash

        member this.priceConvert stock option = failwith "implement"

    
    member this.europeanPrice (rate:decimal) (direction:float) (ref:decimal) (vol:decimal) (strike:decimal) (expiry:DateTime) (legType:OptionKind) (buyingDate: DateTime) =
        let leg = BasicOptions.buildLeg legType (float strike) direction European expiry buyingDate
        let stockInfo = {
            Rate = float rate
            Volatility = float vol
            CurrentPrice = float ref
        }
        bsPricer.blackScholes stockInfo leg

