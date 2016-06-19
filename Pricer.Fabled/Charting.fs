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

type XAxis = 
    abstract axisLabel: string -> XAxis
    abstract tickFormat: System.Func<float,string> -> XAxis

type YAxis =
    abstract axisLabel: string -> YAxis
    abstract tickFormat: System.Func<float,string> -> YAxis

type LineChart = 
    abstract xAxis: XAxis
    abstract yAxis: YAxis
    abstract useInteractiveGuideline: bool -> LineChart
    abstract transitionDuration: bool -> LineChart
    abstract showLegend: bool -> LineChart
    abstract showXAxis: bool -> LineChart
    abstract showYAxis: bool -> LineChart
    

type models = 
    abstract lineChart: unit -> LineChart

[<Erase>]
module nv =
    let models: models = failwith "JS only"
    let addGraph: System.Func<LineChart> -> unit = 
        failwith "JS Only"

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
        
        chart.yAxis.axisLabel("Voltage (v)").tickFormat(D3.Globals.format(",.2f")) |> ignore
        chart

    let drawLineChart (data: LineData array) = 
        nv.addGraph(new System.Func<LineChart>(fun () -> 
            let chart = genrateChart data
            D3.Globals.select("#payoffchart").datum(data).call(chart) |> ignore
            chart
        ))

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