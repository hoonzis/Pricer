namespace Pricer.Fabled

open System
open Fable.Core
open Fable.Import
open Fable.Import.Browser
open Fable.Core.JsInterop

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
    abstract tickFormat: System.Func<Object,string> -> Axis
    abstract tickSize: int -> Axis

[<AbstractClass>]
type Chart() = 
    abstract xAxis: Axis
    abstract yAxis: Axis
    abstract forceY: float array -> Chart
    abstract showLegend: bool -> Chart
    abstract showXAxis: bool -> Chart
    abstract showYAxis: bool -> Chart
    abstract color: string[] -> Chart
    abstract margin: obj -> Chart

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

    let prepareLineChart xLabel yLabel (data: Series<Value> array) height = 
        let max = data |> Array.collect (fun serie -> serie.values) |> Array.maxBy (fun v-> v.y)
        let min = data |> Array.collect (fun serie -> serie.values) |> Array.minBy (fun v-> v.y)
        let maxY = Math.Round(max.y + (0.1 * max.y))
        let minY = Math.Ceiling(min.y - abs(0.1 * min.y))
        
        let margin = createObj [
                        "left" ==> 80
                        "right" ==> 80
                    ]

        let range = RangeUtils.range minY maxY
        let chart = 
                nv.models
                    .lineChart()
                    .useInteractiveGuideline(true)
                    .margin(margin)
                    .showLegend(true)
                    .showXAxis(true)
                    .showYAxis(true)
                    .forceY(range)

        chart.xAxis.axisLabel(xLabel).tickFormat(D3.Globals.format(".f")) |> ignore
        chart.yAxis.axisLabel(yLabel).tickFormat(D3.Globals.format(".1f"))|> ignore
        chart


    let clearAndGetParentChartDiv (selector:string) =
        let element = D3.Globals.select(selector);
        element.html("") |> ignore
        element
    
    let drawChart (chart:Chart) (data: Object) (chartSelector: string) height = 
        let chartElement = clearAndGetParentChartDiv(chartSelector)
        chartElement.style("height",sprintf "%ipx" height) |> ignore
        chartElement.datum(data).call(chart) |> ignore


    let drawLineChart (data: Series<Value> array) (chartSelector:string) xLabel yLabel =      
        let height = 500
        let chart = prepareLineChart xLabel yLabel data height
        drawChart chart data chartSelector height
    
    let drawDateScatter (data: Series<DateScatterValue> array) (chartSelector:string) xLabel yLabel = 
        let colors = D3.Scale.Globals.category10()
        let chart = nv.models.scatterChart().pointRange([|10.0;800.0|]).showLegend(true).showXAxis(true).showYAxis(true).color(colors.range())
        let timeFormat = D3.Time.Globals.format("%x")
        chart.yAxis.axisLabel(yLabel) |> ignore
        chart.xAxis.tickFormat(fun x -> 
            let dateValue = DateUtils.fromTicks(x :?> int)
            timeFormat.Invoke(dateValue)
        ).axisLabel(xLabel) |> ignore
        drawChart chart data chartSelector