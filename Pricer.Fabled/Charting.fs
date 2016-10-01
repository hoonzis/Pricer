namespace Pricer.Fabled

open System
open Fable.Core
open Fable.Import.Browser
open Fable.Import
open Pricer.Core

type Value = {
    x: int
    y: float
}

type DateScatterValue = {
    x: DateTime
    y: float
    size: float
}

type Series<'a> = {
    key: string
    values: 'a array
}

type Axis = 
    abstract axisLabel: string -> Axis
    abstract tickFormat: System.Func<float,string> -> Axis

[<AbstractClass>]
type Chart() = 
    abstract xAxis: Axis
    abstract yAxis: Axis
    abstract showLegend: bool -> Chart
    abstract showXAxis: bool -> Chart
    abstract showYAxis: bool -> Chart

[<AbstractClass>]
type LineChart() = inherit Chart()
        with member __.useInteractiveGuideline (value:bool): Chart = failwith "JSOnly"

[<AbstractClass>]
type ScatterChart() = inherit Chart()
    with member __.pointRange(value: double array): ScatterChart = failwith "JSOnly"

        
type models = 
    abstract lineChart: unit -> LineChart
    abstract scatterChart: unit -> ScatterChart

[<Erase>]
module nv =
    let models: models = failwith "JS only"

module Charting =
 
    let tuplesToPoints (data: (float*float) list): Value array = 
            data |> List.map (fun (x,y) -> 
                {
                    Value.x = int x
                    Value.y = y
                }
            ) |> Array.ofList

    let buildLines (data:(Leg*(float*float) list) seq)= 
        data |> Seq.map (fun (leg,linedata) -> 
            {
                key = leg.Definition.Name
                values = linedata |> tuplesToPoints
            })

    let prepareLineChart = 
        let chart = nv.models.lineChart().useInteractiveGuideline(true).showLegend(true).showXAxis(true)
        chart.xAxis.axisLabel("Underlying Price").tickFormat(D3.Globals.format(",.1f")) |> ignore
        chart.yAxis.axisLabel("Profit").tickFormat(D3.Globals.format(",.1f")) |> ignore
        chart


    let clearAndGetParentChartDiv (selector:string) =
        let element = D3.Globals.select(selector);
        element.html("") |> ignore
        element
    
    let drawChart (chart:Chart) (data: Object) (chartSelector: string) = 
        let chartElement = clearAndGetParentChartDiv(chartSelector)
        chartElement.style("height","500px") |> ignore
        chartElement.datum(data).call(chart) |> ignore


    let drawLineChart (data: Series<Value> array) (chartSelector:string) =      
        let chart = prepareLineChart
        drawChart chart data chartSelector

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

    let legAndPricingToDataPoint (l,pricing) = 
        {
            x = l.Expiry
            y = l.Strike
            size = pricing.Premium
        }
        
    let drawScatter (data: Series<DateScatterValue> array) (chartSelector:string) = 
        let chart = nv.models.scatterChart().pointRange([|10.0;800.0|]).showLegend(true).showXAxis(true)
        chart.yAxis.axisLabel("Strike") |> ignore

        chart.xAxis.tickFormat(D3.Globals.format("%x")).axisLabel("Expiry") |> ignore
        drawChart chart data chartSelector