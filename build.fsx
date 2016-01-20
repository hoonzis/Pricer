// include Fake lib
#r @"tools\FAKE\tools\FakeLib.dll"

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
let projectSummary = "Library which contains several methods to price options and estimate historical volatility"
let projectDescription = "Pricer for options and other financial products"

// version info
//I will have to add AssemblyInfo fsharp style and upgrade version from it
let version = "0.8.0"
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

Target "Test" (fun _ ->
    !! (testDir + @"\OptionsPricingTests.dll")
      |> NUnit (fun p ->
                 {p with
                   DisableShadowCopy = true;
                   OutputFile = testDir + @"TestResults.xml"})
)


Target "CreatePackage" (fun _ ->
  let net45Dir = packagingDir @@ "lib/net45/"

  CleanDirs [net45Dir]

  CopyFile net45Dir (buildDir @@ "OptionsPricing.dll")
  CopyFile net45Dir (buildDir @@ "OptionsPricing.XML")
  CopyFile net45Dir (buildDir @@ "OptionsPricing.pdb")

  trace (sprintf "Pushing Nuget Package using Key:%s" nugetKey)
  NuGet (fun p ->
      {p with
          Authors = authors
          Project = projectName
          Description = projectDescription
          OutputPath = deployDir
          Summary = projectSummary
          WorkingDir = packagingDir
          Version = version
          AccessKey = nugetKey
          Publish = hasBuildParam "nugetkey" })
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
  ==> "Test"
  ==> "Zip"
  ==> "CreatePackage"

"CompileApp"
  ==> "CompileTest"
  ==> "Test"
  ==> "CreatePackage"

// start build
RunTargetOrDefault "CompileTest"
