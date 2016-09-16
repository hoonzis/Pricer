@echo off
cls
"tools\NuGet.exe" "Install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
"tools\FAKE\tools\Fake.exe" build.fsx %*
