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
    Premium = None
    Style = American
    PurchaseDate = DateTime.Now
}

let startVal = (DateTime.Now.AddDays -30.0).ToString("yyyy-MM-dd")
let endVal = DateTime.Now.ToString("yyyy-MM-dd")
let url = sprintf "https://www.quandl.com/api/v1/datasets/LSE/%s.json?start_date=%s&end_date=%s&auth_token=VP8Yryjzdv-3kxtyH5Uc" "VOD" startVal endVal

let testStockData =
    let ticker,data = Stocks.lseStock "VOD" (DateTime.Now.AddDays -30.0) DateTime.Now
    let sorted = data |> Seq.sortBy (fun (data,_) -> data)
    
    let floating = 
        data |> Seq.windowed 10
             |> Seq.map (fun window -> (window |> Seq.head |> fst), (window |> Seq.head |> fst), window |> Seq.averageBy (fun (_,value) -> value))
             |> Seq.map (fun (windowStart, windowEnd,avg) -> windowStart.AddDays(float (windowEnd- windowStart).Days),avg)
    
    Chart.Combine [
        Chart.Line(data)
        Chart.Line(floating)
    ]
    
let testHistogramOfRatios = 
    let ticker,data = Stocks.lseStock "VOD" (DateTime.Now.AddDays -30.0) DateTime.Now
    let logRatiosData = Stocks.logRatios data
    let histData = FinCharts.histogramChart logRatiosData 20
    Chart.Bar(histData,"Histogram")

let testButterflyChart =
    let stock = Stocks.stockInfo "VOD" (DateTime.Now.AddDays -10.0) DateTime.Now
    let strategy = StrategiesExamples.butterfly stock

    let yMark = ChartTypes.TickMark(Interval = 5.0,Enabled = true)
    let yMinor = ChartTypes.TickMark(Interval = 1.0,Enabled = true)
    let xMinor = ChartTypes.TickMark(Interval = 1.0,Enabled = true)
    let strategyData,legsData = Options.getStrategyData strategy

    let strategyLine = Chart.Line(strategyData,Name = strategy.Name) |> Chart.WithSeries.Style(Color = Color.Red, BorderWidth = 5)
    let legsLines = legsData |> Seq.mapi (fun i (leg,legData) -> Chart.Line(legData,sprintf "Leg %i %s" i leg.Name) |> Chart.WithSeries.Style(Color = Color.Black, BorderWidth = 2))
    let allLines = legsLines |> Seq.append [strategyLine]
    
    let chart = Chart.Combine(allLines) |> Chart.WithLegend(true) 
    chart