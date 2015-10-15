namespace OptionsPricing

open System
open System.Globalization
open System.Collections.Generic
open MathNet.Numerics.Statistics
open MathNet.Numerics.Distributions
open System.Net
open System.IO


module Stocks =

    let openingPrice tick = tick.Open
    let closingPrice tick = tick.Close
    let tradingDay tick = tick.Date

    let closingVsOpenVolEstimate (data:seq<Tick>) =
        let ab = data |> Seq.sortBy tradingDay
                      |> Seq.pairwise
                      |> Seq.map (fun (tick0,tick1) ->
                            if tick0.Open.IsNone || tick1.Open.IsNone then failwith "We don't have opening values"
                            (log tick1.Close/tick1.Open.Value, log tick1.Open.Value/tick0.Close)
                        )

        let statsA = DescriptiveStatistics(ab|>Seq.map(fun(a,b)->a),false)
        let statsB = DescriptiveStatistics(ab|>Seq.map(fun(a,b)->b),false)
        let tradingDays = float statsA.Count
        let volatility = sqrt ((statsA.Variance + statsB.Variance)*tradingDays)
        volatility

    let closingLogRatios (data:seq<Tick>) =
        data |> Seq.sortBy (fun tick -> tick.Date)
             |> Seq.pairwise
             |> Seq.map (fun (tick0,tick1) -> log (tick1.Close/tick0.Close))

    let estimateVolFromReturns (logRatios:IEnumerable<float>) =
        let stats = DescriptiveStatistics(logRatios,false)
        let dailyVolatility = stats.StandardDeviation
        let tradingDays = float stats.Count
        let volatility = dailyVolatility * sqrt tradingDays
        let drift = (stats.Mean * tradingDays) + (pown volatility 2) / 2.0
        (volatility,drift)

    let estimateVol estimationType stockData =
        match estimationType with
            | CloseVsClose -> estimateVolFromReturns (closingLogRatios stockData) |> fst
            | CloseVsOpen -> closingVsOpenVolEstimate stockData

    let randomPrice drift volatility dt initial (dist:Normal) =
        // Calculate parameters of the exponential
        let driftExp = (drift - 0.5 * pown volatility 2) * dt
        let randExp = volatility * (sqrt dt)

        // Recursive loop that actually generates the price
        let rec loop price = seq {
            yield price
            let price = price * exp (driftExp + randExp * dist.Sample())
            yield! loop price }

        // Return path starting at 'initial'
        loop initial


    let simulatePrice exchange ticker startDate endDate =
        let name,data = MarketData.stock exchange ticker startDate endDate
        let (vol, drift) = data |> closingLogRatios |> estimateVolFromReturns
        let dist = Normal(0.0, 1.0)
        let dates = data |> Seq.map (fun tick->tick.Date.DayOfYear) |> List.ofSeq |> List.rev
        let firstClose = data |> Seq.minBy tradingDay |> closingPrice
        let randoms = randomPrice drift vol 0.005 firstClose dist
        Seq.zip dates randoms

    let stockInfo exchange ticker startDate endDate =
        let name,data = MarketData.stock exchange ticker startDate endDate
        let (vol, drift) = data |> List.ofSeq |> closingLogRatios |> estimateVolFromReturns
        {
            Rate = 0.03
            Volatility = vol
            CurrentPrice = data |> Seq.maxBy tradingDay |> closingPrice
        }

    let floatingAvg (n:int) (data:seq<Tick>) =
        data |> Seq.sortBy tradingDay |> Seq.windowed n
             |> Seq.map (fun window -> (window |> Seq.head |> tradingDay), (window |> Seq.last |> tradingDay), window |> Seq.averageBy closingPrice)
             |> Seq.map (fun (windowStart, windowEnd,avg) -> windowStart.AddDays(float (windowEnd- windowStart).Days),avg)

    let stockFloatingAvg exchange ticker =
        let ticker,data = MarketData.stock exchange ticker (Some (DateTime.Now.AddDays -365.0)) (Some DateTime.Now)
        let dayCloses = data |> Seq.sortBy tradingDay |> Seq.map (fun tick -> (tick.Date,tick.Close))
        let averages = [
            "daily", dayCloses
            "10 - days", floatingAvg 10 data
            "30 - days", floatingAvg 30 data
            "60 - days", floatingAvg 60 data
        ]
        averages
