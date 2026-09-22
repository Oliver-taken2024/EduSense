<#
    Startar API + UI bundna mot 0.0.0.0 istället för localhost, öppnar
    brandväggsportarna en gång (idempotent) och sätter EDUSENSE_LAN_ORIGIN
    så CORS-policyn (Program.cs) släpper igenom kollegans webbläsare.
    Kör: högerklicka -> "Kör med PowerShell" (kräver admin för
    brandväggsregeln första gången), eller från ett admin-PowerShell:
    .\start-lan-dev.ps1
#>

$ErrorActionPreference = "Stop"

$apiPort = 5215
$uiPort  = 5107
$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)

# Första privata IPv4-adressen som inte är loopback/APIPA - har du flera
# nätverkskort (VPN, Hyper-V, WSL) kan fel adress plockas; sätt då $lanIp
# manuellt istället för raden nedan.
$lanIp = (Get-NetIPAddress -AddressFamily IPv4 |
    Where-Object { $_.IPAddress -notlike "169.254.*" -and $_.IPAddress -ne "127.0.0.1" -and $_.PrefixOrigin -ne "WellKnown" } |
    Select-Object -First 1 -ExpandProperty IPAddress)

if (-not $lanIp) {
    throw "Hittade ingen LAN-IP automatiskt. Kör 'ipconfig' och sätt `$lanIp manuellt i skriptet."
}

Write-Host "LAN-IP: $lanIp" -ForegroundColor Cyan

if ($isAdmin) {
    foreach ($rule in @(
        @{ Name = "EduSense LAN Dev API"; Port = $apiPort },
        @{ Name = "EduSense LAN Dev UI";  Port = $uiPort }
    )) {
        if (-not (Get-NetFirewallRule -DisplayName $rule.Name -ErrorAction SilentlyContinue)) {
            New-NetFirewallRule -DisplayName $rule.Name -Direction Inbound -Protocol TCP -LocalPort $rule.Port -Action Allow | Out-Null
            Write-Host "Brandväggsregel skapad: $($rule.Name) (port $($rule.Port))"
        }
    }
} else {
    Write-Warning "Inte admin - hoppar över brandväggsregler. Kör skriptet som admin första gången, annars kan kollegan inte nå portarna."
}

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:EDUSENSE_LAN_ORIGIN    = "http://${lanIp}:$uiPort"

Start-Process powershell -WorkingDirectory $repoRoot -ArgumentList "-NoExit", "-Command", "`$env:ASPNETCORE_URLS='http://0.0.0.0:$apiPort'; dotnet run --project EduSense.API --no-launch-profile"
Start-Process powershell -WorkingDirectory $repoRoot -ArgumentList "-NoExit", "-Command", "`$env:ASPNETCORE_URLS='http://0.0.0.0:$uiPort'; dotnet run --project EduSense.UI --no-launch-profile"

Write-Host ""
Write-Host "Ge kollegan den här länken: http://${lanIp}:$uiPort" -ForegroundColor Green
