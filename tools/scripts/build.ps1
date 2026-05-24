$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location $root
Write-Host "Restoring and building PtpLabClock.App..." -ForegroundColor Cyan
dotnet restore .\PtpLabClock.sln
dotnet build .\src\PtpLabClock.App\PtpLabClock.App.csproj -c Debug
Write-Host "Build complete. Run Visual Studio as Administrator for RAW Npcap mode." -ForegroundColor Green
