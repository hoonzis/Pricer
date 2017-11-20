#r "bin/Debug/Pricer.Core.dll"
#r "bin/Debug/Pricer.dll"
#r "bin/Debug/Pricer.Benchmark.exe"
#r "../packages/BenchmarkDotNet.Core/lib/net46/BenchmarkDotNet.Core.dll" 
#r "../packages/BenchmarkDotNet/lib/net46/BenchMarkDotNet.dll" 
#r "../packages/BenchmarkDotNet.Toolchains.Roslyn/lib/net46/BenchmarkDotNet.Toolchains.Roslyn.dll"
#r "../packages/BenchmarkDotNet.Diagnostics.Windows/lib/net46/BenchmarkDotNet.Diagnostics.Windows.dll"
#r "../packages/Microsoft.DotNet.PlatformAbstractions/lib/net45/Microsoft.DotNet.PlatformAbstractions.dll"
#r "../packages/Microsoft.DotNet.InternalAbstractions/lib/net451/Microsoft.DotNet.InternalAbstractions.dll"
#r "../packages/System.Threading.Tasks.Extensions/lib/netstandard1.0/System.Threading.Tasks.Extensions.dll"
#r "../packages/Microsoft.CodeAnalysis.CSharp/lib/netstandard1.3/Microsoft.CodeAnalysis.CSharp.dll"
#r "../packages/Microsoft.CodeAnalysis.Common/lib/netstandard1.3/Microsoft.CodeAnalysis.dll"
#r "../packages/System.IO.FileSystem/lib/net46/System.IO.FileSystem.dll" 
#r "../packages/System.Security.Cryptography.Algorithms/lib/net46/System.Security.Cryptography.Algorithms.dll" 
#r "../packages/System.Security.Cryptography.Primitives/lib/net46/System.Security.Cryptography.Primitives.dll" 

open Pricer.Benchmark
open BenchmarkDotNet.Running;

let summary = BenchmarkRunner.Run<BinomialBenchmark>();
printfn "%A" summary