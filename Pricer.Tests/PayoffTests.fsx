#r "bin/Debug/Pricer.Core.dll"
#r "bin/Debug/Pricer.dll"
#r "bin/Debug/Pricer.Fabled.exe"
#load "../packages/FSharp.Charting/FSharp.Charting.fsx"
#r "../packages/MathNet.Numerics/lib/net40/MathNet.Numerics.dll"

open System
open Pricer.Core
open Pricer
open Pricer.Fabled
open FSharp.Charting.ChartTypes
open FSharp.Charting
open System.Windows.Forms.DataVisualization
open System.Drawing
open MathNet.Numerics.Statistics

let option =  {
    Strike = 250.0
    Expiry = DateTime.Now.AddDays(90.)
    Direction = 1.0
    Kind = Call
    Style = European
    PurchaseDate = DateTime.Now
}

let option2 =  {
    Strike = 265.0
    Expiry = DateTime.Now.AddDays(90.)
    Direction = 1.0
    Kind = Put
    Style = European
    PurchaseDate = DateTime.Now
}

let stock = {
  CurrentPrice = 201.0
  Volatility = 0.25
  Rate = 0.03
}

let strategy = {
    Name = "Strategy One"
    Stock = stock
    Legs = 
        [
            { Definition = Option option; Pricing = None}
            { Definition = Option option2; Pricing = None}
        ]
}

let realProvider = new MathNetProvider() :> IMathProvider
let pricer = new SimplePricer() :> IPricer
let payoffGenerator = new PayoffsGenerator(pricer)

let chartSingleYear strategy (payoff:PayoffChartData) =
    let strategyLine = 
        Chart.Line(payoff.StrategySerie ,Name = strategy.Name) 
        |> Chart.WithSeries.Style(Color = Color.Red, BorderWidth = 5)

    let legsLines = payoff.LegsSeries |> Seq.mapi (fun i (leg,legData) -> Chart.Line(legData,(sprintf "Leg %i %s" i leg.Definition.Name)))
    let allLines = legsLines |> Seq.append [strategyLine]
    let chart = Chart.Combine allLines |> Chart.WithLegend(true)
    chart


let data = payoffGenerator.getStrategyData strategy


chartSingleYear strategy data



let simpleProvider = new SimpleMathProvider() :> IMathProvider
let getLine valueProvider name = 
    let data = [-1.0.. 0.01 .. 1.0] |> List.map (fun x-> 
        let y = valueProvider x
        x, y
    )
    Chart.Line(data,name)

let lines = seq {
    yield getLine SimpleMath.erf1 "ERF"
    yield getLine simpleProvider.cdf "ERF - to - CDF"
    yield getLine realProvider.cdf "CDF"
}

let chart= Chart.Combine lines |> Chart.WithLegend(true)