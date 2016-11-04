namespace Pricer.Fabled

open System
open Fable.Core
open Fable.Import
open Pricer.Core
open Fable.Core.JsInterop

module ChartingTest =
    
    let random = new Random()

    let randomValues() = 
        [|1 .. 10|] |> Array.map (fun i -> 
        {
            x = new DateTime(2014, i, 1)
            y = float (random.Next() / 100000)
            size = float (random.Next())
        })

    let drawChart() = 
        let series = [|
                {
                    key = "Series 1"
                    values = randomValues()
                };
                {
                    key = "Series 2"
                    values = randomValues()
                }
            |]

        Charting.drawDateScatter series "#chart" "X axis" "Y axis"

    drawChart()