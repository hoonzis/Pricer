namespace Pricer.Fabled

open Fable.Core
open Fable.Import.Browser
open Fable.Import
open Pricer.Core

type Value = {
    x: int
    y: float
}

type LineData = {
    key: string
    values: Value array
}

type Axis = 
    abstract axisLabel: string -> Axis
    abstract tickFormat: System.Func<float,string> -> Axis

type LineChart = 
    abstract xAxis: Axis
    abstract yAxis: Axis
    abstract useInteractiveGuideline: bool -> LineChart   
    abstract showLegend: bool -> LineChart
    abstract showXAxis: bool -> LineChart
    abstract showYAxis: bool -> LineChart
    

type models = 
    abstract lineChart: unit -> LineChart

[<Erase>]
module nv =
    let models: models = failwith "JS only"

module Charting =
 
    let tuplesToPoints (data: (float*float) list) = 
            data |> List.map (fun (x,y) -> 
                {
                    x = int x
                    y = y
                }
            ) |> Array.ofList

    let buildLines (data:(Leg*(float*float) list) seq)= 
        data |> Seq.map (fun (leg,linedata) -> 
            {
                key = leg.Definition.Name
                values = linedata |> tuplesToPoints
            })

    let genrateChart (data:LineData array) = 
        let chart = nv.models.lineChart().useInteractiveGuideline(true).showLegend(true).showXAxis(true)

        chart.xAxis.axisLabel("Underlying Price").tickFormat(D3.Globals.format(",.1f")) |> ignore
        
        chart.yAxis.axisLabel("Profit").tickFormat(D3.Globals.format(",.1f")) |> ignore
        chart

    let drawLineChart (data: LineData array) =      
        let chart = genrateChart data
        let chartElement = D3.Globals.select("#payoffchart")
        chartElement.style("height","300px") |> ignore
        chartElement.datum(data).call(chart) |> ignore

    let drawPayoff data =
        let payoff = 
            match data with
                | SingleYear (strategyData, legsData) ->
                    let legLines = buildLines legsData
                    let strategyLine = {
                        key = "Strategy"
                        values = strategyData |> tuplesToPoints
                    }
                        
                    seq {
                        yield! legLines
                        yield strategyLine
                    } |> Array.ofSeq
                | _ -> failwith "not implemented"

        drawLineChart payoff        