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
  "CONTRIBUTING.md",
  "SECURITY.md",
  "SUPPORT.md",
  "CODE_OF_CONDUCT.md",
  "CHANGELOG.md",
  ".editorconfig",
  "global.json",
  ".github\CODEOWNERS",
  ".github\PULL_REQUEST_TEMPLATE.md",
  ".github\dependabot.yml",
  "src\PtpLabClock.App\PtpLabClock.App.csproj",
  "src\PtpLabClock.Core\PtpLabClock.Core.csproj",
  "src\PtpLabClock.Protocol\PtpLabClock.Protocol.csproj",
  "src\PtpLabClock.Pcap\PtpLabClock.Pcap.csproj",
  "src\PtpLabClock.Reporting\PtpLabClock.Reporting.csproj",
  "tests\PtpLabClock.Protocol.Tests\PtpLabClock.Protocol.Tests.csproj",
  ".github\workflows\build.yml",
  ".github\workflows\release.yml",
  ".github\workflows\codeql.yml",
  ".github\workflows\dependency-review.yml",
  ".github\workflows\scorecard.yml",
  ".github\workflows\reuse-lint.yml",
  "docs\index.md",
  "docs\installation.md",
  "docs\raw-nic-mode.md",
  "docs\protocol-validation.md",
  "docs\wireshark-validation.md",
  "docs\limitations.md",
  "docs\public-readiness-checklist.md",
  "tools\scripts\package-release.ps1"
)
foreach ($item in $required) {
  $path = Join-Path $root $item
  if (-not (Test-Path $path)) { throw "Missing: $item" }
  Write-Host "OK  $item"
}
Write-Host "Layout check completed." -ForegroundColor Green
