# Starts Home.WebApi and Home.WebUI concurrently
# API:    https://localhost:57174
# WebUI:  https://localhost:7019

$ApiProject = "Home.WebApi"
$WebProject = "Home.WebUI"

Write-Host "Starting $ApiProject..." -ForegroundColor Cyan
$Api = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot'; dotnet run --project $ApiProject" -PassThru

Write-Host "Starting $WebProject..." -ForegroundColor Cyan
$Web = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot'; dotnet run --project $WebProject" -PassThru

Write-Host ""
Write-Host "Both projects started." -ForegroundColor Green
Write-Host "  API:   https://localhost:57174/swagger" -ForegroundColor Yellow
Write-Host "  WebUI: https://localhost:7019" -ForegroundColor Yellow
Write-Host ""
Write-Host "Close this window or press Ctrl+C to stop everything." -ForegroundColor Gray

try {
    Wait-Process -Id $Api.Id, $Web.Id
} catch {
    Stop-Process -Id $Api.Id -ErrorAction SilentlyContinue
    Stop-Process -Id $Web.Id -ErrorAction SilentlyContinue
}
