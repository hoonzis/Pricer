namespace Pricer

open System
open MathNet.Numerics.Statistics

module FinCharts = 
    let histogramChart data bucketCount = 
        let histoData = new Histogram(data,bucketCount)
        let chartData = [for i in 0 .. bucketCount-1 -> histoData.Item i]
        let tuples = chartData |> Seq.map (fun bucket -> bucket.LowerBound + (bucket.UpperBound - bucket.LowerBound)/2.0,bucket.Count)
        tuples