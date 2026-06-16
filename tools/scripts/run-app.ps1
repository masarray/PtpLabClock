# SPDX-License-Identifier: Apache-2.0
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root
Write-Host "Starting PtpLabClock.App..." -ForegroundColor Cyan
dotnet run --project .\src\PtpLabClock.App\PtpLabClock.App.csproj
