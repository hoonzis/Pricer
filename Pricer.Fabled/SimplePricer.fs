namespace Pricer.Fabled

open System
open Pricer.Core

type SimplePricer()= 
    
    let bsPricer = new BlackScholesPricer(new SimpleMathProvider())

    interface IPricer with
        member this.priceOption stock option = bsPricer.blackScholes stock option

        member this.priceCash cash = {
            Premium = cash.Price
            Delta = 1.0
        }

        member this.priceConvert stock option = failwith "implement CB pricing"