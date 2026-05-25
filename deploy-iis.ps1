param(
    [string]$DeployRoot = "C:\inetpub\CoreAppwithSSO",
    [int]$ApiPort       = 8080,
    [int]$DashboardPort = 8081,
    [int]$HrPort        = 4201,
    [int]$FinancePort   = 4202,
    [int]$InventoryPort = 4203
)

# Self-elevate if not running as Administrator
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Start-Process powershell.exe "-ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs -Wait
    exit
}

$ErrorActionPreference = "Stop"
$SrcRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# ---------- Verify prerequisites ----------
Write-Host "`n=== Checking prerequisites ===" -ForegroundColor Cyan

# Check IIS feature
$iis = Get-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
if ($iis.State -ne 'Enabled') {
    Write-Error "IIS is not installed. Enable it via: Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer -All"
}

# Check URL Rewrite Module
$rewriteDll = "$env:SystemRoot\System32\inetsrv\rewrite.dll"
if (-not (Test-Path $rewriteDll)) {
    Write-Warning "IIS URL Rewrite Module not found. Angular SPA routing will NOT work."
    Write-Warning "Download from: https://www.iis.net/downloads/microsoft/url-rewrite"
}

# Check ASP.NET Core Module
$ancmDll = "$env:ProgramFiles\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll"
if (-not (Test-Path $ancmDll)) {
    Write-Warning "ASP.NET Core Module V2 not found. Install the .NET 10 Hosting Bundle."
    Write-Warning "Download from: https://dotnet.microsoft.com/download/dotnet/10.0"
}

Import-Module WebAdministration -ErrorAction Stop

# ---------- Stop app pool before copying (API DLL is otherwise locked by IIS) ----------
$poolName = "CoreAppwithSSOPool"
if (Test-Path "IIS:\AppPools\$poolName") {
    $state = (Get-WebAppPoolState -Name $poolName).Value
    if ($state -ne 'Stopped') {
        Write-Host "`n=== Stopping app pool $poolName (was $state) ===" -ForegroundColor Cyan
        Stop-WebAppPool -Name $poolName
        Start-Sleep -Seconds 2
    }
}

# ---------- Copy files ----------
Write-Host "`n=== Copying published files ===" -ForegroundColor Cyan

$sites = @{
    "api"       = "$SrcRoot\publish\api"
    "dashboard" = "$SrcRoot\core-app-with-sso-ui\dist\core-app-with-sso-ui\browser"
    "hr"        = "$SrcRoot\core-app-with-sso-ui\dist\hr\browser"
    "finance"   = "$SrcRoot\core-app-with-sso-ui\dist\finance\browser"
    "inventory" = "$SrcRoot\core-app-with-sso-ui\dist\inventory\browser"
}

foreach ($name in $sites.Keys) {
    $dest = "$DeployRoot\$name"
    if (-not (Test-Path $dest)) { New-Item -Path $dest -ItemType Directory -Force | Out-Null }
    Write-Host "  Copying $name -> $dest"
    Copy-Item -Path "$($sites[$name])\*" -Destination $dest -Recurse -Force
}

# Ensure logs dir for API
$logsDir = "$DeployRoot\api\logs"
if (-not (Test-Path $logsDir)) { New-Item -Path $logsDir -ItemType Directory -Force | Out-Null }

# ---------- Create App Pool ----------
Write-Host "`n=== Creating application pool ===" -ForegroundColor Cyan

if (-not (Test-Path "IIS:\AppPools\$poolName")) {
    New-WebAppPool -Name $poolName | Out-Null
    Write-Host "  Created pool: $poolName"
} else {
    Write-Host "  Pool already exists: $poolName"
}

# .NET CLR = No Managed Code (for ASP.NET Core in-process hosting)
Set-ItemProperty "IIS:\AppPools\$poolName" -Name managedRuntimeVersion -Value ""
Set-ItemProperty "IIS:\AppPools\$poolName" -Name startMode -Value "AlwaysRunning"

# ---------- Create IIS Sites ----------
Write-Host "`n=== Creating IIS websites ===" -ForegroundColor Cyan

$siteConfigs = @(
    @{ Name = "CoreAppwithSSO-API";       Port = $ApiPort;       Path = "$DeployRoot\api" },
    @{ Name = "CoreAppwithSSO-UI";        Port = $DashboardPort; Path = "$DeployRoot\dashboard" },
    @{ Name = "CoreAppwithSSO-HR";        Port = $HrPort;        Path = "$DeployRoot\hr" },
    @{ Name = "CoreAppwithSSO-Finance";   Port = $FinancePort;   Path = "$DeployRoot\finance" },
    @{ Name = "CoreAppwithSSO-Inventory"; Port = $InventoryPort; Path = "$DeployRoot\inventory" }
)

foreach ($cfg in $siteConfigs) {
    $existing = Get-Website -Name $cfg.Name -ErrorAction SilentlyContinue
    if ($existing) {
        Write-Host "  Updating: $($cfg.Name) -> port $($cfg.Port)"
        Set-ItemProperty "IIS:\Sites\$($cfg.Name)" -Name physicalPath -Value $cfg.Path
    } else {
        Write-Host "  Creating: $($cfg.Name) -> port $($cfg.Port)"
        New-Website -Name $cfg.Name `
                    -Port $cfg.Port `
                    -PhysicalPath $cfg.Path `
                    -ApplicationPool $poolName `
                    -Force | Out-Null
    }
}

# ---------- Set folder permissions ----------
Write-Host "`n=== Setting folder permissions ===" -ForegroundColor Cyan

$acl = Get-Acl $DeployRoot
$poolIdentity = "IIS AppPool\$poolName"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $poolIdentity, "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
)
$acl.SetAccessRule($rule)
Set-Acl -Path $DeployRoot -AclObject $acl

# API needs write for logs
$apiLogsAcl = Get-Acl "$DeployRoot\api\logs"
$writeRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $poolIdentity, "Modify", "ContainerInherit,ObjectInherit", "None", "Allow"
)
$apiLogsAcl.SetAccessRule($writeRule)
Set-Acl -Path "$DeployRoot\api\logs" -AclObject $apiLogsAcl

Write-Host "  Granted ReadAndExecute to $poolIdentity on $DeployRoot"
Write-Host "  Granted Modify to $poolIdentity on $DeployRoot\api\logs"

# ---------- Start sites ----------
Write-Host "`n=== Starting websites ===" -ForegroundColor Cyan

foreach ($cfg in $siteConfigs) {
    Start-Website -Name $cfg.Name -ErrorAction SilentlyContinue
    Write-Host "  Started: $($cfg.Name)"
}

# ---------- Summary ----------
Write-Host "`n=== Deployment complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "  API (Swagger):  http://localhost:$ApiPort/swagger"
Write-Host "  Dashboard:      http://localhost:$DashboardPort"
Write-Host "  HR App:         http://localhost:$HrPort"
Write-Host "  Finance App:    http://localhost:$FinancePort"
Write-Host "  Inventory App:  http://localhost:$InventoryPort"
Write-Host ""
Write-Host "  Deploy root:    $DeployRoot"
Write-Host "  App pool:       $poolName"
Write-Host ""
Write-Host "IMPORTANT:" -ForegroundColor Yellow
Write-Host "  1. Ensure the app pool identity ($poolIdentity) has access to SQL Server."
Write-Host "     Run in SQL Server: CREATE LOGIN [$poolIdentity] FROM WINDOWS;"
Write-Host "                        ALTER SERVER ROLE sysadmin ADD MEMBER [$poolIdentity];"
Write-Host "  2. If you changed ports, update:"
Write-Host "     - appsettings.Production.json -> Cors:Origins"
Write-Host "     - environment.prod.ts -> apiBaseUrl (then rebuild Angular apps)"
Write-Host ""
Read-Host "Press Enter to close"
