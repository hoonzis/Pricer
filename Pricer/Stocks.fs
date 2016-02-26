namespace Pricer

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
