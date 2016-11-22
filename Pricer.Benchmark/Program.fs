namespace Pricer.Benchmark

open BenchmarkDotNet
open BenchmarkDotNet.Running

module OptionsBenchmark = 

    [<EntryPoint>]
    let main argv = 
        let summary = BenchmarkRunner.Run<BinomialBenchmark>()
        printf "%A" summary
        System.Console.Read() |> ignore
        0