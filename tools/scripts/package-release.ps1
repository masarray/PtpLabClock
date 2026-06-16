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


$appHash = (Get-FileHash .\artifacts\PtpLabClock.App.win-x64.portable.exe -Algorithm SHA256).Hash.ToLowerInvariant()
$consoleHash = (Get-FileHash .\artifacts\PtpLabClock.Console.win-x64.portable.exe -Algorithm SHA256).Hash.ToLowerInvariant()
$createdUtc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")

@"
## Process Bus Timing Lab local portable build

Windows PTP lab simulator and timing-health monitor for IEC 61850 FAT, SAT, analyzer validation, and Process Bus troubleshooting.

### Verification

````powershell
Get-FileHash .\PtpLabClock.App.win-x64.portable.exe -Algorithm SHA256
Get-Content .\checksums.txt
````

Expected direct EXE hashes from this local build:

````text
$appHash  PtpLabClock.App.win-x64.portable.exe
$consoleHash  PtpLabClock.Console.win-x64.portable.exe
````

### Known limitations

- Portable EXE artifacts are not code-signed yet and may trigger Windows SmartScreen on first run.
- RAW NIC mode depends on Npcap, adapter driver support, and local security policy.
- Local self-capture failure does not always prove packet transmission failed; verify with external capture when possible.
- Software timestamps are diagnostic only and are not hardware-timestamped timing evidence.
- This project is not a certified PTP grandmaster, GPS clock, or relay-acceptance timing source.

_Generated locally at $createdUtc._
"@ | Set-Content .\artifacts\release-notes.md -Encoding UTF8

Write-Host "Portable release files written to artifacts/:" -ForegroundColor Green
Get-ChildItem .\artifacts -File | Sort-Object Name | Select-Object Name, Length | Format-Table -AutoSize
