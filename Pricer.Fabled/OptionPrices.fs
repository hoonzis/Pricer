namespace Pricer.Fabled

open Fable.Core
open Fable.Import
open Pricer.Core
open Fable.Core.JsInterop

module OptionPrices =
    let pricer = new SimplePricer()
    let optionsAnalyzer = new OptionsAnalyzer(pricer)
    
    type OptionPricesViewModel() = 
        let mutable stock = new StockViewModel(StrategiesExamples.exampleStock)
        let mutable pricesTable = [||];

        let toTableValues kind values = 
            createObj [
                "strike" ==> values.y
                "expiry" ==> values.x.ToShortTimeString()
                "price" ==> values.size
                "kind" ==> kind
            ]

        member __.updatePrices =
            let kinds = [|Call;Put|]
            let pricesPerOptionKind = kinds |> Array.map (fun kind -> 
                let prices = optionsAnalyzer.optionPricesTriples stock.buildStock kind
                let legsWithPrices = 
                    prices |> Seq.choose (fun leg ->
                        match leg.Definition, leg.Pricing with
                            | Option option, Some pricing -> Some (option, pricing)
                            | _ -> None
                        ) 
                        |> Array.ofSeq
                let dataPoints = legsWithPrices |> Array.map Charting.legAndPricingToDataPoint
                let serie = {
                    key = kind.ToString()
                    values = dataPoints
                }
                
                serie
            )

            pricesTable <-
                pricesPerOptionKind 
                    |> Array.map (fun serie -> 
                        let fromSerie = toTableValues serie.key
                        let withOptionKind = serie.values |> Array.map fromSerie
                        withOptionKind
                    ) |> Array.collect id

            Charting.drawScatter pricesPerOptionKind "#optionPricesChart"

    let extraOpts =
        createObj [
            "el" ==> ".optionPricesApp"           
        ]

    let vm = OptionPricesViewModel()
    let app = VueHelper.createFromObj(vm, extraOpts)
    

