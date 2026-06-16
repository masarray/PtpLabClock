# SPDX-License-Identifier: Apache-2.0
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root

$artifacts = Join-Path $root "artifacts"
$publish = Join-Path $root "publish"
if (Test-Path $artifacts) { Remove-Item $artifacts -Recurse -Force }
if (Test-Path $publish) { Remove-Item $publish -Recurse -Force }
New-Item -ItemType Directory -Path $artifacts | Out-Null
New-Item -ItemType Directory -Path $publish | Out-Null
New-Item -ItemType Directory -Path (Join-Path $publish "app") | Out-Null
New-Item -ItemType Directory -Path (Join-Path $publish "console") | Out-Null

Write-Host "Restoring and testing..." -ForegroundColor Cyan
dotnet restore .\PtpLabClock.sln
dotnet test .\PtpLabClock.sln -c Release --no-restore

$singleFileProps = @(
  "-p:PublishSingleFile=true",
  "-p:SelfContained=true",
  "-p:IncludeNativeLibrariesForSelfExtract=true",
  "-p:EnableCompressionInSingleFile=true",
  "-p:PublishReadyToRun=false",
  "-p:PublishTrimmed=false",
  "-p:DebugType=None",
  "-p:DebugSymbols=false"
)

Write-Host "Publishing self-contained single EXE artifacts..." -ForegroundColor Cyan
dotnet publish .\src\PtpLabClock.App\PtpLabClock.App.csproj -c Release -r win-x64 --self-contained true @singleFileProps -o .\publish\app
dotnet publish .\src\PtpLabClock.Console\PtpLabClock.Console.csproj -c Release -r win-x64 --self-contained true @singleFileProps -o .\publish\console

Copy-Item .\publish\app\PtpLabClock.App.exe .\artifacts\PtpLabClock.App.win-x64.portable.exe -Force
Copy-Item .\publish\console\PtpLabClock.Console.exe .\artifacts\PtpLabClock.Console.win-x64.portable.exe -Force

$packages = @(
  @{ Name = "PtpLabClock.App.win-x64.portable"; Exe = ".\artifacts\PtpLabClock.App.win-x64.portable.exe" },
  @{ Name = "PtpLabClock.Console.win-x64.portable"; Exe = ".\artifacts\PtpLabClock.Console.win-x64.portable.exe" }
)

foreach ($package in $packages) {
  $dir = Join-Path ".\artifacts" $package.Name
  New-Item -ItemType Directory -Path $dir -Force | Out-Null
  Copy-Item $package.Exe (Join-Path $dir (Split-Path $package.Exe -Leaf)) -Force
  Copy-Item .\LICENSE $dir -Force
  Copy-Item .\NOTICE $dir -Force
  Copy-Item .\THIRD-PARTY-NOTICES.md $dir -Force
  Copy-Item .\docs\release-readme.md (Join-Path $dir "README-FIRST.md") -Force
  Compress-Archive -Path (Join-Path $dir "*") -DestinationPath ".\artifacts\$($package.Name).zip" -Force
}

Get-ChildItem .\artifacts -File | Where-Object { $_.Extension -in ".exe", ".zip" } | Sort-Object Name | ForEach-Object {
  $hash = Get-FileHash $_.FullName -Algorithm SHA256
  "$($hash.Hash.ToLowerInvariant())  $($_.Name)"
} | Set-Content .\artifacts\checksums.txt -Encoding UTF8

Write-Host "Portable release files written to artifacts/:" -ForegroundColor Green
Get-ChildItem .\artifacts -File | Sort-Object Name | Select-Object Name, Length | Format-Table -AutoSize
