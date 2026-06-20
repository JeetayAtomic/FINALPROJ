[CmdletBinding()]
param(
    [string]$TargetRoot = 'C:\inetpub\wwwroot\CoreAppwithSSO',
    [string]$SiteName   = 'CoreAppwithSSO',
    [int]   $Port       = 8080,
    [string]$AppPool    = 'CoreAppwithSSOPool',
    [switch]$SkipBuild,
    [switch]$SkipInstall,
    [switch]$CreateSite
)

$ErrorActionPreference = 'Stop'
Set-Location -Path $PSScriptRoot

$apps = @(
    @{ Name = 'shell';     Source = 'dist\core-app-with-sso-ui\browser'; Target = '' }
)

function Test-Admin {
    $id = [Security.Principal.WindowsIdentity]::GetCurrent()
    (New-Object Security.Principal.WindowsPrincipal $id).IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not $SkipInstall) {
    Write-Host '==> npm ci' -ForegroundColor Cyan
    npm ci
    if ($LASTEXITCODE -ne 0) { throw 'npm ci failed' }
}

if (-not $SkipBuild) {
    Write-Host '==> npm run build' -ForegroundColor Cyan
    npm run build
    if ($LASTEXITCODE -ne 0) { throw 'build failed' }
}

foreach ($app in $apps) {
    $src = Join-Path $PSScriptRoot $app.Source
    if (-not (Test-Path $src)) { throw "Missing build output: $src" }
}

if (-not (Test-Path $TargetRoot)) {
    Write-Host "==> creating $TargetRoot" -ForegroundColor Cyan
    New-Item -ItemType Directory -Path $TargetRoot -Force | Out-Null
}

foreach ($app in $apps) {
    $src = (Join-Path $PSScriptRoot $app.Source).TrimEnd('\')
    $dst = if ($app.Target) { Join-Path $TargetRoot $app.Target } else { $TargetRoot }

    Write-Host "==> deploying $($app.Name) -> $dst" -ForegroundColor Cyan

    if (-not (Test-Path $dst)) {
        New-Item -ItemType Directory -Path $dst -Force | Out-Null
    }

    & xcopy /E /I /Y /Q "$src\*" "$dst\" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "xcopy failed for $($app.Name)" }
}

Write-Host "`nDeployed to $TargetRoot" -ForegroundColor Green

if ($CreateSite) {
    if (-not (Test-Admin)) { throw 'CreateSite requires an elevated PowerShell session.' }

    Write-Host "`n==> configuring IIS site '$SiteName' on port $Port" -ForegroundColor Cyan
    Import-Module WebAdministration

    if (-not (Test-Path "IIS:\AppPools\$AppPool")) {
        New-WebAppPool -Name $AppPool | Out-Null
    }
    Set-ItemProperty "IIS:\AppPools\$AppPool" -Name managedRuntimeVersion -Value ''
    Set-ItemProperty "IIS:\AppPools\$AppPool" -Name managedPipelineMode   -Value Integrated

    if (Test-Path "IIS:\Sites\$SiteName") {
        Write-Host "    site exists — updating physical path + binding" -ForegroundColor DarkGray
        Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath        -Value $TargetRoot
        Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool     -Value $AppPool
    } else {
        New-Website -Name $SiteName `
                    -PhysicalPath $TargetRoot `
                    -ApplicationPool $AppPool `
                    -Port $Port `
                    -Force | Out-Null
    }

    foreach ($app in $apps) {
        if (-not $app.Target) { continue }
        $appPath = "/$($app.Target)"
        $physical = Join-Path $TargetRoot $app.Target
        $iisPath  = "IIS:\Sites\$SiteName$appPath"

        if (Test-Path $iisPath) {
            Set-ItemProperty $iisPath -Name physicalPath    -Value $physical
            Set-ItemProperty $iisPath -Name applicationPool -Value $AppPool
        } else {
            New-WebApplication -Site $SiteName -Name $app.Target `
                               -PhysicalPath $physical `
                               -ApplicationPool $AppPool | Out-Null
        }
    }

    Start-WebAppPool -Name $AppPool -ErrorAction SilentlyContinue
    Start-Website    -Name $SiteName -ErrorAction SilentlyContinue

    Write-Host "`nIIS site '$SiteName' is live at:" -ForegroundColor Green
    Write-Host "  http://localhost:$Port/"          -ForegroundColor Green
} else {
    Write-Host "`nSkipped IIS site creation. Re-run elevated with -CreateSite to provision." -ForegroundColor Yellow
}
