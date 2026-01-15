$ErrorActionPreference = "Stop"
$env:MSBUILD_EXE_PATH = $null

dotnet build
