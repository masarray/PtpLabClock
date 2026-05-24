# SPDX-License-Identifier: GPL-3.0-or-later
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Write-Host "Process Bus Timing Lab layout check" -ForegroundColor Cyan
$required = @(
  "LICENSE",
  "AGENTS.md",
  "ROADMAP.md",
  "src\PtpLabClock.App\PtpLabClock.App.csproj",
  "src\PtpLabClock.Core\PtpLabClock.Core.csproj",
  "src\PtpLabClock.Protocol\PtpLabClock.Protocol.csproj",
  "src\PtpLabClock.Pcap\PtpLabClock.Pcap.csproj"
)
foreach ($item in $required) {
  $path = Join-Path $root $item
  if (-not (Test-Path $path)) { throw "Missing: $item" }
  Write-Host "OK  $item"
}
Write-Host "Layout check completed. Now run: dotnet build" -ForegroundColor Green
