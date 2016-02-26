#r "bin/Debug/Pricer.dll"
#load "../packages/FSharp.Charting/FSharp.Charting.fsx"
#r "../packages/MathNet.Numerics/lib/net40/MathNet.Numerics.dll"

open System
open Pricer
open FSharp.Charting.ChartTypes
open FSharp.Charting
open System.Windows.Forms.DataVisualization
open System.Drawing
open MathNet.Numerics.Statistics

let option = {
    Strike = 250.0
    Expiry = DateTime.Now.AddDays(90.)
    Direction = 1.0
    Kind = Call
    Style = European
    PurchaseDate = DateTime.Now
}

//get current price, volatility and interest free rate for VOD from London Stock Exchange
//let stock = Stocks.stockInfo LSE "VOD" (Some (DateTime.Now.AddDays -60.0)) (Some DateTime.Now)

let stock = {
  CurrentPrice = 201.0
  Volatility = 0.124
  Rate = 0.03
}

let convert = { 
    Coupon = 0.4
    ReferencePrice = 200.0
    Direction = 1.0
    ConversionRatio = 1.0
    Maturity = DateTime.Now.AddDays(40.0)
    FaceValue = 10000.0
}

let convertStrategy = {
    Stock = stock
    Legs = [
        {
            Definition = Convertible convert
            Pricing = None
        }
    ]
    Name = "Convertible Example"
}

let chartData data strategy =
    match data with
        | SingleYear (strategyData, legsData) ->
            let strategyLine = Chart.Line(strategyData,Name = strategy.Name) |> Chart.WithSeries.Style(Color = Color.Red, BorderWidth = 5)
            let legsLines = legsData |> Seq.mapi (fun i (leg,legData) -> Chart.Line(legData,leg.Definition.Name))
            let allLines = legsLines |> Seq.append [strategyLine]
            let chart = Chart.Combine allLines |> Chart.WithLegend(true)
            chart
        | MultiYear (strategyPerYear) ->
            let allYearLines = strategyPerYear |> Seq.map (fun strategyData -> 
                Chart.Line(strategyData,Name = strategy.Name) |> Chart.WithSeries.Style(Color = Color.Red, BorderWidth = 5)
            )

            let chart = Chart.Combine allYearLines |> Chart.WithLegend(true)
            chart


let strategy = StrategiesExamples.callSpread stock
let data = Options.getStrategyData strategy
chartData data strategy

let convertibleData = Options.getStrategyData convertStrategy
chartData convertibleData convertStrategy


