# SPDX-License-Identifier: Apache-2.0
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Write-Host "Process Bus Timing Lab layout check" -ForegroundColor Cyan
$required = @(
  "LICENSE",
  "NOTICE",
  "AGENTS.md",
  "ROADMAP.md",
  "THIRD-PARTY-NOTICES.md",
  "src\PtpLabClock.App\PtpLabClock.App.csproj",
  "src\PtpLabClock.Core\PtpLabClock.Core.csproj",
  "src\PtpLabClock.Protocol\PtpLabClock.Protocol.csproj",
  "src\PtpLabClock.Pcap\PtpLabClock.Pcap.csproj",
  "src\PtpLabClock.Reporting\PtpLabClock.Reporting.csproj",
  "tests\PtpLabClock.Protocol.Tests\PtpLabClock.Protocol.Tests.csproj",
  ".github\workflows\build.yml",
  ".github\workflows\release.yml"
)
foreach ($item in $required) {
  $path = Join-Path $root $item
  if (-not (Test-Path $path)) { throw "Missing: $item" }
  Write-Host "OK  $item"
}
Write-Host "Layout check completed." -ForegroundColor Green
