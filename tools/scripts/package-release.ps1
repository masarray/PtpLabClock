# SPDX-License-Identifier: Apache-2.0
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root

$artifacts = Join-Path $root "artifacts"
if (Test-Path $artifacts) { Remove-Item $artifacts -Recurse -Force }
New-Item -ItemType Directory -Path $artifacts | Out-Null

Write-Host "Restoring and testing..." -ForegroundColor Cyan
dotnet restore .\PtpLabClock.sln
dotnet test .\PtpLabClock.sln -c Release --no-restore

dotnet publish .\src\PtpLabClock.App\PtpLabClock.App.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=false -o .\artifacts\PtpLabClock.App.win-x64.framework-dependent
dotnet publish .\src\PtpLabClock.App\PtpLabClock.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:PublishReadyToRun=false -o .\artifacts\PtpLabClock.App.win-x64.self-contained
dotnet publish .\src\PtpLabClock.Console\PtpLabClock.Console.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:PublishReadyToRun=false -o .\artifacts\PtpLabClock.Console.win-x64.self-contained

foreach ($dir in Get-ChildItem .\artifacts -Directory) {
  Copy-Item .\LICENSE $dir.FullName -Force
  Copy-Item .\NOTICE $dir.FullName -Force
  Copy-Item .\THIRD-PARTY-NOTICES.md $dir.FullName -Force
  Copy-Item .\docs\release-readme.md (Join-Path $dir.FullName "README-FIRST.md") -Force
  Compress-Archive -Path (Join-Path $dir.FullName "*") -DestinationPath ".\artifacts\$($dir.Name).zip" -Force
}

Get-ChildItem .\artifacts -Filter *.zip | Sort-Object Name | ForEach-Object {
  $hash = Get-FileHash $_.FullName -Algorithm SHA256
  "$($hash.Hash.ToLowerInvariant())  $($_.Name)"
} | Set-Content .\artifacts\checksums.txt -Encoding UTF8

Write-Host "Release packages written to artifacts/." -ForegroundColor Green
