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

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; deployDir]
)

Target "CompilePricer" (fun _ ->
    [@"Pricer/Pricer.fsproj"]
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "CompileMarketData" (fun _ ->
    [@"Pricer.MarketData\Pricer.MarketData.fsproj"]
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
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
// Dependenciesf

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
  ==> "Test"
  ==> "CreatePackage"

// start build
RunTargetOrDefault "Test"