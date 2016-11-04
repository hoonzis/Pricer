namespace Pricer.Fabled

open System
open Fable.Core
open Fable.Import
open Fable.Import.Browser
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
    abstract tickFormat: System.Func<Object,string> -> Axis

module DateUtils = 
    [<Emit("new Date($0)")>]
    let fromTicks (ticks: int): DateTime = jsNative

[<AbstractClass>]
type Chart() = 
    abstract xAxis: Axis
    abstract yAxis: Axis
    abstract showLegend: bool -> Chart
    abstract showXAxis: bool -> Chart
    abstract showYAxis: bool -> Chart
    abstract color: string[] -> Chart

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

    let drawPayoff (strategyData, legsData) =
        let legLines = buildLines legsData
        let strategyLine = {
            key = "Strategy"
            values = strategyData |> tuplesToPoints
        }
               
        let payoff = seq {
            yield! legLines
            yield strategyLine
        } 

        drawLineChart (payoff |> Array.ofSeq)

    let legAndPriceToScatterPoint (l,price) = 
        {
            x = l.Expiry
            y = l.Strike
            size = price
        }
        
    let drawDateScatter (data: Series<DateScatterValue> array) (chartSelector:string) xLabel yLabel = 
        let colors = D3.Scale.Globals.category10()
        let chart = nv.models.scatterChart().pointRange([|10.0;800.0|]).showLegend(true).showXAxis(true).color(colors.range())
        let timeFormat = D3.Time.Globals.format("%x")
        chart.yAxis.axisLabel(yLabel) |> ignore
        chart.xAxis.tickFormat(fun x -> 
            let dateValue = DateUtils.fromTicks(x :?> int)
            timeFormat.Invoke(dateValue)
        ).axisLabel(xLabel) |> ignore
        drawChart chart data chartSelector