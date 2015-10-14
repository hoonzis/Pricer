#r "bin/Debug/OptionsPricing.dll"
#load "../packages/FSharp.Charting.0.90.12/FSharp.Charting.fsx"
#r "../packages/MathNet.Numerics.3.8.0/lib/net40/MathNet.Numerics.dll"

open System
open OptionsPricing
open FSharp.Charting.ChartTypes
open FSharp.Charting
open System.Windows.Forms.DataVisualization
open System.Drawing
open MathNet.Numerics.Statistics

let option = {
    Strike = 250.0
    Expiry = new DateTime(2015,12,12)
    Direction = 1.0
    Kind = Call
    Style = American
    PurchaseDate = DateTime.Now
}

let testFloatingWindows =
    let ticker,data = MarketData.stock LSE "VOD" (Some (DateTime.Now.AddDays -60.0)) (Some DateTime.Now)
    let tenDaysAvg = Stocks.floatingAvg 10 data
    let fiveDaysAvg = Stocks.floatingAvg 5 data
    Chart.Combine [
        Chart.Line tenDaysAvg
        Chart.Line fiveDaysAvg
    ]

let testButterflyChart =
    let stock = Stocks.stockInfo LSE "VOD" (Some (DateTime.Now.AddDays -60.0)) (Some DateTime.Now)
    let strategy = StrategiesExamples.callSpread stock
    let strategyData,legsData = Options.getStrategyData strategy
    let strategyLine = Chart.Line(strategyData,Name = strategy.Name) |> Chart.WithSeries.Style(Color = Color.Red, BorderWidth = 5)
    let legsLines = legsData |> Seq.mapi (fun i (leg,legData) -> Chart.Line(legData,sprintf "Leg %i %s" i leg.Definition.Name) |> Chart.WithSeries.Style(Color = Color.Black, BorderWidth = 2))
    let allLines = legsLines |> Seq.append [strategyLine]

    let chart = Chart.Combine allLines |> Chart.WithLegend(true)
    chart
