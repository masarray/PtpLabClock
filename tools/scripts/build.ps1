# SPDX-License-Identifier: Apache-2.0
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root

Write-Host "Validating repository layout..." -ForegroundColor Cyan
.\tools\scripts\validate-layout.ps1

Write-Host "Restoring solution..." -ForegroundColor Cyan
dotnet restore .\PtpLabClock.sln

Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build .\PtpLabClock.sln -c Release --no-restore

Write-Host "Running tests..." -ForegroundColor Cyan
dotnet test .\PtpLabClock.sln -c Release --no-build

Write-Host "Running protocol smoke validation..." -ForegroundColor Cyan
dotnet run --project .\src\PtpLabClock.Console --configuration Release -- --validate-protocol --domain 0

Write-Host "Build, test, and protocol validation complete." -ForegroundColor Green
