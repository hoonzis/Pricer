namespace Pricer.Core

open System
open Pricer
open MathNet.Numerics.Distributions

module MonteCarloPricer = 

    let price (stock:StockInfo) (option:OptionLeg) steps =
        let dt = option.TimeToExpiry / (float steps)
        let normal = new Normal()
        let randomPath = Stocks.randomPrice stock normal dt |> Seq.take steps
        let avgStockPrice = randomPath |> Seq.average
        let price = BasicOptions.optionValue option avgStockPrice
        price
        

    let run (stock:StockInfo) (option:OptionLeg) simulations =
        let timesteps = 1000

        let avgPrices = seq {
            while true do
                yield price stock option timesteps
        }

        let price = avgPrices |> Seq.take simulations |> Seq.average
        { 
            Premium = price
            Delta = 0.0
        }