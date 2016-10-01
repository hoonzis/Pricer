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
        
        member __.updatePrices =
            let kinds = [|Call;Put|]
            let pricesPerOptionKind = kinds |> Array.map (fun kind -> 
                let prices = optionsAnalyzer.optionPricesTripes stock.buildStock kind
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

            Charting.drawScatter pricesPerOptionKind "#optionPricesChart"


    let extraOpts =
        createObj [
            "el" ==> ".optionPricesApp"           
        ]

    let vm = OptionPricesViewModel()
    let app = VueHelper.createFromObj(vm, extraOpts)
    

