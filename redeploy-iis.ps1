param(
    [string]$DeployRoot = "C:\inetpub\CoreAppwithSSO"
)

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Start-Process powershell.exe "-ExecutionPolicy Bypass -File `"$PSCommandPath`" -DeployRoot `"$DeployRoot`"" -Verb RunAs -Wait
    exit
}

$ErrorActionPreference = "Stop"
Import-Module WebAdministration

$SrcRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$pool = "CoreAppwithSSOPool"

$sites = @{
    "api"       = "$SrcRoot\publish\api"
    "dashboard" = "$SrcRoot\core-app-with-sso-ui\dist\core-app-with-sso-ui\browser"
    "hr"        = "$SrcRoot\core-app-with-sso-ui\dist\hr\browser"
    "finance"   = "$SrcRoot\core-app-with-sso-ui\dist\finance\browser"
    "inventory" = "$SrcRoot\core-app-with-sso-ui\dist\inventory\browser"
}

if ((Get-WebAppPoolState -Name $pool).Value -ne 'Stopped') {
    Write-Host "Stopping app pool $pool..."
    Stop-WebAppPool -Name $pool
    Start-Sleep -Seconds 2
}

foreach ($name in $sites.Keys) {
    $src = $sites[$name]
    $dest = "$DeployRoot\$name"
    if (-not (Test-Path $src)) { Write-Warning "  Skipping $name - source not found: $src"; continue }
    if (-not (Test-Path $dest)) { New-Item -Path $dest -ItemType Directory -Force | Out-Null }
    Write-Host "Copying $name..."
    Copy-Item -Path "$src\*" -Destination $dest -Recurse -Force
}

Write-Host "Starting app pool $pool..."
Start-WebAppPool -Name $pool

$dll = Get-Item "$DeployRoot\api\CoreAppwithSSO.API.dll"
Write-Host "Done. API DLL: $($dll.LastWriteTime) ($($dll.Length) bytes)" -ForegroundColor Green
