#r "bin/Debug/Pricer.Core.dll"
#r "bin/Debug/Pricer.dll"
#r "bin/Debug/Pricer.Benchmark.exe"

#r "../packages/BenchmarkDotNet/lib/net45/BenchmarkDotNet.dll"
#r "../packages/BenchmarkDotNet.Core/lib/net45/BenchmarkDotNet.Core.dll"
#r "../packages/BenchmarkDotNet.Toolchains.Roslyn/lib/net45/BenchmarkDotNet.Toolchains.Roslyn.dll"
#r "../packages/BenchmarkDotNet.Diagnostics.Windows/lib/net45/BenchmarkDotNet.Diagnostics.Windows.dll"
#r "../packages/Microsoft.CodeAnalysis.CSharp/lib/net45/Microsoft.CodeAnalysis.CSharp.dll"
#r "../packages/Microsoft.CodeAnalysis.Common/lib/net45/Microsoft.CodeAnalysis.dll" 
#r "../packages/System.Reflection.Metadata/lib/netstandard1.1/System.Reflection.Metadata.dll"
#r "../packages/System.Collections.Immutable/lib/netstandard1.0/System.Collections.Immutable.dll"
#r "../packages/System.Threading.Tasks.Extensions/lib/netstandard1.0/System.Threading.Tasks.Extensions.dll"


open Pricer.Core
open Pricer
open Pricer.Benchmark
open BenchmarkDotNet.Running

let summary = BenchmarkRunner.Run<BinomialBenchmark>();
printfn "%A" summary