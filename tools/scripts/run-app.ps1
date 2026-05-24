$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location $root
Write-Host "Starting PtpLabClock.App..." -ForegroundColor Cyan
dotnet run --project .\src\PtpLabClock.App\PtpLabClock.App.csproj
