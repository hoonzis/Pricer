namespace Pricer.Fabled

open System
open Fable.Core
open Fable.Import
open Pricer.Core
open Fable.Core.JsInterop

module OptionPrices =
    let pricer = new SimplePricer()
    let optionsAnalyzer = new OptionsAnalyzer(pricer)
    
    type OptionPricesViewModel() = 
        let mutable stock = new StockViewModel(StrategiesExamples.exampleStock)
        let mutable optionsTable = [||];

        member __.updatePrices() =
            let stockValue = stock.buildStock
            let puts = optionsAnalyzer.optionPricesExamples stockValue Put |> Array.ofSeq
            let calls = optionsAnalyzer.optionPricesExamples stockValue Call |> Array.ofSeq

            let scatterCalls = calls |> Array.map Charting.legAndPriceToScatterPoint
            let scatterPuts = puts |> Array.map Charting.legAndPriceToScatterPoint
            
            let series = [|
                {
                    key = Call.ToString()
                    values = scatterCalls
                };
                {
                    key = Put.ToString()
                    values = scatterPuts
                }
            |]
            
            Charting.drawScatter series "#optionPricesChart"

            let merged = puts |> Array.zip calls |> Array.map (fun ((put, putPrice), (call, callPrice)) -> 
                createObj [
                    "strike" ==> (Fable.Import.JS.Number.Create put.Strike).toFixed(2.0)
                    "expiry" ==> (put.Expiry |> Tools.toDate)
                    "putPrice" ==> (Fable.Import.JS.Number.Create putPrice).toFixed(4.0)
                    "callPrice" ==> (Fable.Import.JS.Number.Create callPrice).toFixed(4.0)
                ]
            )

            optionsTable <- merged

    let extraOpts =
        createObj [
            "el" ==> ".optionPricesApp"           
        ]

    let vm = OptionPricesViewModel()
    let app = VueHelper.createFromObj(vm, extraOpts)
    

