namespace Pricer.MarketData


open System
open System.IO
open System.Collections.Generic
open FSharp.Data
open System.Net
open Pricer
open MathNet.Numerics.Distributions

module StockAnalysis = 
    let simulatePrice exchange ticker startDate endDate =
            let name,data = MarketProviders.stock exchange ticker startDate endDate
            let (vol, drift) = data |> Stocks.closingLogRatios |> Stocks.estimateVolFromReturns
            let dist = Normal(0.0, 1.0)
            let dates = data |> Seq.map (fun tick->tick.Date.DayOfYear) |> List.ofSeq |> List.rev
            let firstClose = data |> Seq.minBy Stocks.tradingDay |> Stocks.closingPrice
            let randoms = Stocks.randomPrice drift vol 0.005 firstClose dist
            Seq.zip dates randoms

    let stockInfo exchange ticker startDate endDate =
        let name,data = MarketProviders.stock exchange ticker startDate endDate
        let (vol, drift) = data |> List.ofSeq |> Stocks.closingLogRatios |> Stocks.estimateVolFromReturns
        {
            Rate = 0.03
            Volatility = vol
            CurrentPrice = data |> Seq.maxBy Stocks.tradingDay |> Stocks.closingPrice
        }

    let floatingAvg (n:int) (data:seq<Tick>) =
        data |> Seq.sortBy Stocks.tradingDay |> Seq.windowed n
                |> Seq.map (fun window -> (window |> Seq.head |> Stocks.tradingDay), (window |> Seq.last |> Stocks.tradingDay), window |> Seq.averageBy Stocks.closingPrice)
                |> Seq.map (fun (windowStart, windowEnd,avg) -> windowStart.AddDays(float (windowEnd- windowStart).Days),avg)

    let stockFloatingAvg exchange ticker =
        let ticker,data = MarketProviders.stock exchange ticker (Some (DateTime.Now.AddDays -365.0)) (Some DateTime.Now)
        let dayCloses = data |> Seq.sortBy Stocks.tradingDay |> Seq.map (fun tick -> (tick.Date,tick.Close))
        let averages = [
            "daily", dayCloses
            "10 - days", floatingAvg 10 data
            "30 - days", floatingAvg 30 data
            "60 - days", floatingAvg 60 data
        ]
        averages

