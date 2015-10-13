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

// version info
let version = "0.2"  // or retrieve from CI server

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

Target "Zip" (fun _ ->
    !! (buildDir + "\**\*.*")
        -- "*.zip"
        |> Zip buildDir (deployDir + "Calculator." + version + ".zip")
)

// Dependencies
"Clean"
  ==> "CompileApp"
  ==> "CompileTest"
  ==> "NUnitTest"
  ==> "Zip"
  
// start build
RunTargetOrDefault "CompileTest"