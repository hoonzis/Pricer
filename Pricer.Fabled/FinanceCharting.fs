namespace Pricer.Fabled

open System
open Fable.Core
open Fable.Import
open Fable.Import.Browser
open Pricer.Core

module FinanceCharting =
 
    let drawPayoff (data:PayoffChartData) (selector:string) =
        let strategyLine = {
            key = "Strategy"
            values = data.StrategySerie |> Charting.tuplesToPoints
        }

        let legLines = 
            data.LegsSeries |> Seq.map (fun (leg,linedata) -> 
            {
                key = leg.Definition.Name
                values = linedata |> Charting.tuplesToPoints
            })
               
        let payoff = seq {
            yield! legLines
            yield strategyLine
        } 

        Charting.drawLineChart (payoff |> Array.ofSeq) selector "Underlying price" "Profit"

    let legAndPriceToScatterPoint (l,price) = 
        {
            x = l.Expiry
            y = l.Strike
            size = price
        }