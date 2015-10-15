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

let testFloatingWindows =
    let ticker,data = MarketData.stock LSE "VOD" (Some (DateTime.Now.AddDays -60.0)) (Some DateTime.Now)
    let tenDaysAvg = Stocks.floatingAvg 10 data
    let fiveDaysAvg = Stocks.floatingAvg 5 data
    Chart.Combine [
        Chart.Line((data|>Seq.sortBy Stocks.tradingDay |> Seq.map (fun tick -> tick.Date, tick.Close)),Name = "Day")
        Chart.Line(tenDaysAvg,Name="10 days avg")
        Chart.Line(fiveDaysAvg,Name="5 days avg")
    ] |> Chart.WithLegend true

let testPricing =
    let bsPrice = Options.blackScholes stock option
    let binomialPrice = Options.binomial stock option 2000
    printf "BlackScholes price: %f with delta: %f, Binomial Pricing: %f with delta: %f" bsPrice.Premium bsPrice.Delta binomialPrice.Premium binomialPrice.Delta

let testStrategy =
    let strategy = StrategiesExamples.callSpread stock
    let strategyData,legsData = Options.getStrategyData strategy
    let strategyLine = Chart.Line(strategyData,Name = strategy.Name) |> Chart.WithSeries.Style(Color = Color.Red, BorderWidth = 5)
    let legsLines = legsData |> Seq.mapi (fun i (leg,legData) -> Chart.Line(legData,leg.Definition.Name))
    let allLines = legsLines |> Seq.append [strategyLine]
    let chart = Chart.Combine allLines |> Chart.WithLegend(true)
    chart

let volTests =
  let ticker,data = MarketData.stock EURONEXT "ATI" (Some (DateTime.Now.AddDays -180.0)) (Some DateTime.Now)
  let vol = Stocks.estimateVol CloseVsClose data
  let vol2 = Stocks.estimateVol CloseVsOpen data
  printf "Vol1: %f , Vol2: %f" vol vol2
