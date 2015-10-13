// include Fake lib
#r @"packages\FAKE\tools\FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile

RestorePackages()


// Directories
let buildDir  = @".\build\"
let testDir   = @".\test\"
let deployDir = @".\deploy\"
let packagesDir = @".\packages"
let authors = ["Jan Fajfr"]
let projectName = "Pricer"
let projectSummary = "Library which contains several methods to price options and estimate historical volatility"
let projectDescription = "Pricer for options and other financial products"

// version info
let version = "0.2"  // or retrieve from CI server
let nugetKey = getBuildParamOrDefault "nugetKey" ""

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; deployDir]
)

Target "CompileApp" (fun _ ->
    [@"OptionsPricing/OptionsPricing.fsproj"]
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "CompileTest" (fun _ ->
    [@"OptionsPricingTests\OptionsPricingTests.fsproj"]
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "NUnitTest" (fun _ ->
    !! (testDir + @"\OptionsPricingTests.dll")
      |> NUnit (fun p ->
                 {p with
                   DisableShadowCopy = true;
                   OutputFile = testDir + @"TestResults.xml"})
)


Target "CreatePackage" (fun _ ->
    trace (sprintf "Pushing Nuget Package using Key:%s" nugetKey)
    NuGet (fun p ->
        {p with
            Authors = authors
            Project = projectName
            Description = projectDescription
            OutputPath = deployDir
            Summary = projectSummary
            WorkingDir = buildDir
            Version = version
            AccessKey = nugetKey
            Publish = true })
            "OptionsPricing.nuspec"
)

Target "Zip" (fun _ ->
    !! (buildDir + "\**\*.*")
        -- "*.zip"
        |> Zip buildDir (deployDir + "Pricer." + version + ".zip")
)
// Dependenciesf

"Clean"
  ==> "CompileApp"
  ==> "CompileTest"
  ==> "NUnitTest"
  ==> "Zip"
  ==> "CreatePackage"

"CompileApp"
  ==> "CompileTest"
  ==> "NUnitTest"
  ==> "CreatePackage"

// start build
RunTargetOrDefault "CompileTest"
