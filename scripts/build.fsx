// include Fake lib
#r @"..\packages\FAKE\tools\FakeLib.dll"

let nunitRunnerPath = "packages/NUnit.ConsoleRunner/tools/nunit3-console.exe"

open Fake
open Fake.Testing

RestorePackages()

// Directories
let buildDir  = @".\build\"
let testDir   = @".\test\"
let deployDir = @".\deploy\"
let packagingDir = @".\packaging"

let authors = ["Jan Fajfr"]
let projectName = "Pricer"
let projectSummary = "Library with several methods to price options and estimate historical volatility"
let projectDescription = "Pricer for options and other financial products"

let getVersion() =
    let buildCandidate = (environVar "APPVEYOR_BUILD_NUMBER")
    if buildCandidate = "" || buildCandidate = null then "1.0.0" else (sprintf "1.0.0.%s" buildCandidate)

let nugetKey = getBuildParamOrDefault "nugetKey" ""

let Exec command args =
    let result = Shell.Exec(command, args)
    if result <> 0 then failwithf "%s exited with error %d" command result

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; deployDir]
)

Target "CompilePricerCore" (fun _ ->
    [@"Pricer.Core/Pricer.Core.fsproj"]
      |> MSBuildRelease buildDir "Build"
      |> Log "Pricer Core compilation output: "
)

Target "CompilePricer" (fun _ ->
    [@"Pricer/Pricer.fsproj"]
      |> MSBuildRelease buildDir "Build"
      |> Log "Pricer compilation output: "
)

Target "CompileMarketData" (fun _ ->
    [@"Pricer.MarketData\Pricer.MarketData.fsproj"]
      |> MSBuildDebug buildDir "Build"
      |> Log "Market Data Build - Output: "
)

Target "CompileTest" (fun _ ->
    [@"Pricer.Tests\Pricer.Tests.fsproj"]
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "Test" (fun _ ->
    !! (testDir + @"\Pricer.Tests.dll")
      |> NUnit3 (fun p ->
                 {p with
                   ToolPath = nunitRunnerPath})
)

Target "CompileBenchmark" (fun _ -> 
    [@"Pricer.Benchmark\Pricer.Benchmark.fsproj"]
      |> MSBuildDebug buildDir "Build"
      |> Log "Benchmark Build - Output: "
)

let updateNugetPackage p =  {
    p with
        Authors = authors
        Project = projectName
        Description = projectDescription
        OutputPath = deployDir
        Summary = projectSummary
        WorkingDir = packagingDir
        Version = getVersion()
        AccessKey = nugetKey
        Publish = hasBuildParam "nugetkey"
    }

let copyFiles net4Dir =
    CopyFile net4Dir (buildDir @@ "Pricer.dll")
    CopyFile net4Dir (buildDir @@ "Pricer.xml")
    CopyFile net4Dir (buildDir @@ "Pricer.pdb")
    CopyFile net4Dir (buildDir @@ "Pricer.Core.dll")
    CopyFile net4Dir (buildDir @@ "Pricer.Core.xml")
    CopyFile net4Dir (buildDir @@ "Pricer.Core.pdb")

Target "CreatePackage" (fun _ ->
    CreateDir deployDir
    let net461Dir = packagingDir @@ "lib/net461/"
    CleanDirs [net461Dir]
    copyFiles net461Dir
    trace (sprintf "Pushing Nuget Package using Key:%s" nugetKey)
    NuGet updateNugetPackage "Pricer.nuspec"
)

Target "Zip" (fun _ ->
    !! (buildDir + "\**\*.*")
        -- "*.zip"
        |> Zip buildDir (deployDir + "Pricer." + getVersion() + ".zip")
)

Target "RunBenchmark" (fun _ -> 
    let path = sprintf "%s\\Pricer.Benchmark.exe" buildDir
    Exec path ""
)

// Dependencies

"Clean"
  ==> "CompilePricerCore"

"CompilePricer"
  ==> "CompilePricerCore"
  
"CompilePricer"
  ==> "CreatePackage"
  ==> "CompileMarketData"
  ==> "CompileTest"
  ==> "CompileBenchmark"
  
"CompileTest"
  ==> "Test"

"RunBenchmark"
 ==> "CompileBenchmark"

RunTargetOrDefault "Test"