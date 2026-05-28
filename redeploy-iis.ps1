#requires -RunAsAdministrator
<#
.SYNOPSIS
    Re-deploy CoreAppwithSSO code into the IIS sites previously created by deploy-iis.ps1.

.DESCRIPTION
    Stops sites + app pools, republishes/builds, mirrors files into the IIS folder,
    and restarts. Does NOT create or modify sites/bindings - use deploy-iis.ps1 for that.

.PARAMETER TargetRoot
    Same root used by deploy-iis.ps1.

.PARAMETER Only
    Which apps to redeploy. Default: all. Any subset of: api, sample-apps, ui

.PARAMETER SkipBuild
    Skip build/publish - copy whatever's already in publish/* and dist/*.

.PARAMETER SkipNpmInstall
    Skip `npm ci` (still does `npm run build:all` unless -SkipBuild).
#>
[CmdletBinding()]
param(
    [string]$TargetRoot = 'C:\inetpub\wwwroot\CoreAppwithSSO',
    [ValidateSet('api','sample-apps','ui')]
    [string[]]$Only     = @('api','sample-apps','ui'),
    [switch]$SkipBuild,
    [switch]$SkipNpmInstall
)

$ErrorActionPreference = 'Stop'
Set-Location -Path $PSScriptRoot

$RepoRoot       = $PSScriptRoot
$ApiProject     = Join-Path $RepoRoot 'CoreAppwithSSO.API\CoreAppwithSSO.API.csproj'
$SampleProject  = Join-Path $RepoRoot 'CoreAppwithSSO.SampleApps\CoreAppwithSSO.SampleApps.csproj'
$AngularRoot    = Join-Path $RepoRoot 'core-app-with-sso-ui'

$ApiPublish     = Join-Path $RepoRoot 'publish\api'
$SamplePublish  = Join-Path $RepoRoot 'publish\sample-apps'

$ApiTarget      = Join-Path $TargetRoot 'api'
$SampleTarget   = Join-Path $TargetRoot 'sample-apps'
$UiTarget       = Join-Path $TargetRoot 'ui'

$ApiSite        = 'CoreAppwithSSO-API'
$UiSite         = 'CoreAppwithSSO-UI'
$SampleSite     = 'CoreAppwithSSO-SampleApps'
$ApiPool        = 'CoreAppwithSSO-API'
$UiPool         = 'CoreAppwithSSO-UI'
$SamplePool     = 'CoreAppwithSSO-SampleApps'

$AngularApps = @(
    @{ Name = 'shell';     Source = 'dist\core-app-with-sso-ui\browser'; Target = '' },
    @{ Name = 'hr';        Source = 'dist\hr\browser';                   Target = 'hr' },
    @{ Name = 'finance';   Source = 'dist\finance\browser';              Target = 'finance' },
    @{ Name = 'inventory'; Source = 'dist\inventory\browser';            Target = 'inventory' }
)

function Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }
function Info($msg) { Write-Host "    $msg" -ForegroundColor DarkGray }

Import-Module WebAdministration

$wantApi    = $Only -contains 'api'
$wantSample = $Only -contains 'sample-apps'
$wantUi     = $Only -contains 'ui'

# --- Build / publish (only what we need) ---------------------------------
if (-not $SkipBuild) {
    if ($wantApi) {
        Step "dotnet publish API -> $ApiPublish"
        if (Test-Path $ApiPublish) { Remove-Item $ApiPublish -Recurse -Force }
        dotnet publish $ApiProject -c Release -o $ApiPublish --nologo
        if ($LASTEXITCODE -ne 0) { throw 'API publish failed' }
    }
    if ($wantSample) {
        Step "dotnet publish SampleApps -> $SamplePublish"
        if (Test-Path $SamplePublish) { Remove-Item $SamplePublish -Recurse -Force }
        dotnet publish $SampleProject -c Release -o $SamplePublish --nologo
        if ($LASTEXITCODE -ne 0) { throw 'SampleApps publish failed' }
    }
    if ($wantUi) {
        Push-Location $AngularRoot
        try {
            if (-not $SkipNpmInstall) {
                Step 'npm ci'
                npm ci
                if ($LASTEXITCODE -ne 0) { throw 'npm ci failed' }
            }
            Step 'npm run build:all (production)'
            npm run build:all
            if ($LASTEXITCODE -ne 0) { throw 'Angular build failed' }
        } finally {
            Pop-Location
        }
    }
}

# --- Stop just what we're touching ---------------------------------------
$siteMap = @{
    'api'         = @{ Site = $ApiSite;    Pool = $ApiPool    }
    'sample-apps' = @{ Site = $SampleSite; Pool = $SamplePool }
    'ui'          = @{ Site = $UiSite;     Pool = $UiPool     }
}
foreach ($k in $Only) {
    $m = $siteMap[$k]
    if (Test-Path "IIS:\Sites\$($m.Site)") {
        Step "Stopping site $($m.Site)"
        Stop-Website -Name $m.Site -ErrorAction SilentlyContinue
    } else {
        Write-Host "    Warning: site $($m.Site) not found - run deploy-iis.ps1 first" -ForegroundColor Yellow
    }
    if (Test-Path "IIS:\AppPools\$($m.Pool)") {
        Stop-WebAppPool -Name $m.Pool -ErrorAction SilentlyContinue
    }
}
Start-Sleep -Seconds 2

# --- Copy ----------------------------------------------------------------
function CopyTree($src, $dst) {
    if (-not (Test-Path $dst)) { New-Item -ItemType Directory -Path $dst -Force | Out-Null }
    Step "Copy $src -> $dst"
    & robocopy $src $dst /MIR /NFL /NDL /NJH /NJS /NC /NS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "robocopy failed (exit $LASTEXITCODE) for $src -> $dst" }
    $script:LASTEXITCODE = 0
}

if ($wantApi)    { CopyTree $ApiPublish    $ApiTarget    }
if ($wantSample) { CopyTree $SamplePublish $SampleTarget }
if ($wantUi) {
    CopyTree (Join-Path $AngularRoot $AngularApps[0].Source) $UiTarget
    foreach ($app in $AngularApps | Where-Object { $_.Target }) {
        CopyTree (Join-Path $AngularRoot $app.Source) (Join-Path $UiTarget $app.Target)
    }
}

# Ensure logs dirs survive a robocopy /MIR wipe and stay writable.
foreach ($pair in @(@($wantApi, $ApiTarget), @($wantSample, $SampleTarget))) {
    if ($pair[0]) {
        $logs = Join-Path $pair[1] 'logs'
        if (-not (Test-Path $logs)) { New-Item -ItemType Directory -Path $logs -Force | Out-Null }
        & icacls $logs /grant 'IIS_IUSRS:(OI)(CI)M' /T /Q | Out-Null
    }
}

# --- Start ----------------------------------------------------------------
foreach ($k in $Only) {
    $m = $siteMap[$k]
    if (Test-Path "IIS:\AppPools\$($m.Pool)") { Start-WebAppPool -Name $m.Pool }
    if (Test-Path "IIS:\Sites\$($m.Site)")    { Start-Website    -Name $m.Site }
}

Write-Host ''
Write-Host "Redeployed: $($Only -join ', ')" -ForegroundColor Green
