// include Fake lib
#r @"..\tools\FAKE\tools\FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile

RestorePackages()

// Directories
let buildDir  = @".\build\"
let testDir   = @".\test\"
let deployDir = @".\deploy\"
let packagesDir = @".\packages"
let packagingDir = @".\packaging"

let authors = ["Jan Fajfr"]
let projectName = "Pricer"
let projectSummary = "Library with several methods to price options and estimate historical volatility"
let projectDescription = "Pricer for options and other financial products"

// version info
//I will have to add AssemblyInfo fsharp style and upgrade version from it
let version = "0.20.0"
let nugetKey = getBuildParamOrDefault "nugetKey" ""

let Exec command args =
    let result = Shell.Exec(command, args)
    if result <> 0 then failwithf "%s exited with error %d" command result

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; deployDir]
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
      |> NUnit (fun p ->
                 {p with
                   DisableShadowCopy = true;
                   OutputFile = testDir + @"TestResults.xml"})
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
        Version = version
        AccessKey = nugetKey
        Publish = hasBuildParam "nugetkey"
    }

let copyFiles net4Dir =
    CopyFile net4Dir (buildDir @@ "Pricer.dll")
    CopyFile net4Dir (buildDir @@ "Pricer.XML")
    CopyFile net4Dir (buildDir @@ "Pricer.pdb")

Target "CreatePackage" (fun _ ->
    let net4Dir = packagingDir @@ "lib/net40/"
    CleanDirs [net4Dir]
    copyFiles net4Dir
    trace (sprintf "Pushing Nuget Package using Key:%s" nugetKey)
    NuGet updateNugetPackage "Pricer.nuspec"
)

Target "Zip" (fun _ ->
    !! (buildDir + "\**\*.*")
        -- "*.zip"
        |> Zip buildDir (deployDir + "Pricer." + version + ".zip")
)

Target "RunBenchmark" (fun _ -> 
    let path = sprintf "%s\\Pricer.Benchmark.exe" buildDir
    Exec path ""
)

// Dependencies

"Clean"
  ==> "CompilePricer"
  ==> "CompileMarketData"
  ==> "CompileTest"
  ==> "Test"
  ==> "Zip"
  ==> "CreatePackage"

"CompilePricer"
  ==> "CompileMarketData"
  ==> "CompileTest"
  ==> "CompileBenchmark"
  ==> "RunBenchmark"
  
// start build
RunTargetOrDefault "Test"