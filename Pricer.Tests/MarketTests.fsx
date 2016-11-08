#r "bin/Debug/Pricer.MarketData.dll"
#load "../packages/FSharp.Charting/FSharp.Charting.fsx"
#r "../packages/MathNet.Numerics/lib/net40/MathNet.Numerics.dll"

open System
open Pricer.MarketData
open FSharp.Charting.ChartTypes
open FSharp.Charting
open System.Windows.Forms.DataVisualization
open System.Drawing
open MathNet.Numerics.Statistics



let testFloatingWindows =
    let ticker,data = MarketProviders.stock LSE "VOD" (Some (DateTime.Now.AddDays -60.0)) (Some DateTime.Now)
    let tenDaysAvg = StockAnalysis.floatingAvg 10 data
    let fiveDaysAvg = StockAnalysis.floatingAvg 5 data
    Chart.Combine [
        Chart.Line((data|>Seq.sortBy Stocks.tradingDay |> Seq.map (fun tick -> tick.Date, tick.Close)),Name = "Day")
        Chart.Line(tenDaysAvg,Name="10 days avg")
        Chart.Line(fiveDaysAvg,Name="5 days avg")
    ] |> Chart.WithLegend true


let volTests =
  let ticker,data = MarketProviders.stock EURONEXT "ATI" (Some (DateTime.Now.AddDays -180.0)) (Some DateTime.Now)
  let vol = Stocks.estimateVol CloseVsClose data
  let vol2 = Stocks.estimateVol CloseVsOpen data
  printf "Vol1: %f , Vol2: %f" vol vol2
