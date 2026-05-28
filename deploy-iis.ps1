#requires -RunAsAdministrator
<#
.SYNOPSIS
    First-time deploy of CoreAppwithSSO (API + SampleApps + Angular UI) to local IIS.

.DESCRIPTION
    Creates three IIS sites under C:\inetpub\wwwroot\CoreAppwithSSO\:
      - CoreAppwithSSO-API          (port 8080)  -> CoreAppwithSSO.API
      - CoreAppwithSSO-UI           (port 8081)  -> Angular shell + hr/finance/inventory as IIS sub-apps
      - CoreAppwithSSO-SampleApps   (ports 5301, 5302, 5303) -> CoreAppwithSSO.SampleApps
        (one app, three bindings; the controllers gate themselves with [Host("*:530x")])

    Use redeploy-iis.ps1 after this for code-only updates (no IIS provisioning).

.PARAMETER TargetRoot
    Root folder for all deployed artifacts. Defaults to C:\inetpub\wwwroot\CoreAppwithSSO.

.PARAMETER SkipBuild
    Skip dotnet publish + Angular build (use what's already in publish/* and dist/*).

.PARAMETER SkipNpmInstall
    Skip `npm ci` in the Angular project.
#>
[CmdletBinding()]
param(
    [string]$TargetRoot     = 'C:\inetpub\wwwroot\CoreAppwithSSO',
    [int]   $ApiPort        = 8080,
    [int]   $UiPort         = 8081,
    [int[]] $SampleAppPorts = @(5301, 5302, 5303),
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

# --- Sanity checks --------------------------------------------------------
if (-not (Get-Service W3SVC -ErrorAction SilentlyContinue)) {
    throw 'IIS (W3SVC) is not installed. Enable IIS via "Turn Windows features on or off" first.'
}
Import-Module WebAdministration

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'dotnet SDK not found on PATH.'
}
if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    throw 'npm not found on PATH (needed for the Angular build).'
}

# --- Build / publish ------------------------------------------------------
if (-not $SkipBuild) {
    Step "dotnet publish API -> $ApiPublish"
    if (Test-Path $ApiPublish) { Remove-Item $ApiPublish -Recurse -Force }
    dotnet publish $ApiProject -c Release -o $ApiPublish --nologo
    if ($LASTEXITCODE -ne 0) { throw 'API publish failed' }

    Step "dotnet publish SampleApps -> $SamplePublish"
    if (Test-Path $SamplePublish) { Remove-Item $SamplePublish -Recurse -Force }
    dotnet publish $SampleProject -c Release -o $SamplePublish --nologo
    if ($LASTEXITCODE -ne 0) { throw 'SampleApps publish failed' }

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

foreach ($p in @($ApiPublish, $SamplePublish)) {
    if (-not (Test-Path $p)) { throw "Missing publish output: $p (rerun without -SkipBuild)" }
}
foreach ($app in $AngularApps) {
    $src = Join-Path $AngularRoot $app.Source
    if (-not (Test-Path $src)) { throw "Missing Angular build output: $src" }
}

# --- Stop sites/pools before copying (avoid locked DLLs) -----------------
foreach ($s in @($ApiSite, $UiSite, $SampleSite)) {
    if (Test-Path "IIS:\Sites\$s") {
        Step "Stopping site $s"
        Stop-Website -Name $s -ErrorAction SilentlyContinue
    }
}
foreach ($p in @($ApiPool, $UiPool, $SamplePool)) {
    if (Test-Path "IIS:\AppPools\$p") {
        Step "Stopping pool $p"
        Stop-WebAppPool -Name $p -ErrorAction SilentlyContinue
    }
}
# Brief pause so file handles release.
Start-Sleep -Seconds 2

# --- Layout target folder -------------------------------------------------
foreach ($d in @($TargetRoot, $ApiTarget, $SampleTarget, $UiTarget)) {
    if (-not (Test-Path $d)) { New-Item -ItemType Directory -Path $d -Force | Out-Null }
}

function CopyTree($src, $dst) {
    Step "Copy $src -> $dst"
    & robocopy $src $dst /MIR /NFL /NDL /NJH /NJS /NC /NS /NP | Out-Null
    # robocopy exit codes 0-7 are success; >=8 is failure
    if ($LASTEXITCODE -ge 8) { throw "robocopy failed (exit $LASTEXITCODE) for $src -> $dst" }
    $script:LASTEXITCODE = 0
}

CopyTree $ApiPublish    $ApiTarget
CopyTree $SamplePublish $SampleTarget

# Angular: shell at root, sub-apps in subfolders (each is its own IIS app)
$ShellSrc = Join-Path $AngularRoot $AngularApps[0].Source
CopyTree $ShellSrc $UiTarget
foreach ($app in $AngularApps | Where-Object { $_.Target }) {
    $src = Join-Path $AngularRoot $app.Source
    $dst = Join-Path $UiTarget $app.Target
    CopyTree $src $dst
}

# --- IIS provisioning -----------------------------------------------------
function EnsurePool($name) {
    if (-not (Test-Path "IIS:\AppPools\$name")) {
        Step "Creating app pool $name"
        New-WebAppPool -Name $name | Out-Null
    } else {
        Info "App pool $name already exists"
    }
    # No Managed Code (ASP.NET Core runs out-of-CLR via ANCM)
    Set-ItemProperty "IIS:\AppPools\$name" -Name managedRuntimeVersion -Value ''
    Set-ItemProperty "IIS:\AppPools\$name" -Name managedPipelineMode   -Value Integrated
    Set-ItemProperty "IIS:\AppPools\$name" -Name startMode             -Value AlwaysRunning
}

function EnsureSite($name, $physicalPath, $pool, $bindings) {
    if (-not (Test-Path "IIS:\Sites\$name")) {
        $first = $bindings[0]
        Step "Creating site $name on $($first.Protocol):$($first.Port)"
        New-Website -Name $name `
                    -PhysicalPath $physicalPath `
                    -ApplicationPool $pool `
                    -Port $first.Port `
                    -Force | Out-Null
        # First binding was set by New-Website; add any extras.
        foreach ($b in ($bindings | Select-Object -Skip 1)) {
            New-WebBinding -Name $name -Protocol $b.Protocol -Port $b.Port -IPAddress '*' -HostHeader '' | Out-Null
        }
    } else {
        Info "Site $name already exists - updating"
        Set-ItemProperty "IIS:\Sites\$name" -Name physicalPath    -Value $physicalPath
        Set-ItemProperty "IIS:\Sites\$name" -Name applicationPool -Value $pool

        # Sync bindings: remove anything not in $bindings, add anything missing.
        $existing = Get-WebBinding -Name $name
        $wanted   = $bindings | ForEach-Object { "$($_.Protocol)/*:$($_.Port):" }
        foreach ($b in $existing) {
            if ($wanted -notcontains $b.bindingInformation -and `
                $wanted -notcontains ("$($b.protocol)/$($b.bindingInformation)")) {
                Info "Removing stale binding $($b.protocol) $($b.bindingInformation)"
                Remove-WebBinding -Name $name -BindingInformation $b.bindingInformation -Protocol $b.protocol
            }
        }
        foreach ($b in $bindings) {
            $have = Get-WebBinding -Name $name -Protocol $b.Protocol -Port $b.Port -ErrorAction SilentlyContinue
            if (-not $have) {
                Info "Adding binding $($b.Protocol):$($b.Port)"
                New-WebBinding -Name $name -Protocol $b.Protocol -Port $b.Port -IPAddress '*' -HostHeader '' | Out-Null
            }
        }
    }
}

EnsurePool $ApiPool
EnsurePool $UiPool
EnsurePool $SamplePool

EnsureSite $ApiSite    $ApiTarget    $ApiPool    @(@{ Protocol = 'http'; Port = $ApiPort })
EnsureSite $UiSite     $UiTarget     $UiPool     @(@{ Protocol = 'http'; Port = $UiPort  })
EnsureSite $SampleSite $SampleTarget $SamplePool ($SampleAppPorts | ForEach-Object { @{ Protocol = 'http'; Port = $_ } })

# UI sub-applications (hr/finance/inventory) so each has its own web.config (Angular fallback).
# Always remove + recreate: Set-ItemProperty on an existing sub-app's physicalPath
# raises ArgumentNullException via the IIS provider in some states.
foreach ($app in $AngularApps | Where-Object { $_.Target }) {
    $appPath  = "/$($app.Target)"
    $physical = Join-Path $UiTarget $app.Target
    $iisPath  = "IIS:\Sites\$UiSite$appPath"

    if (Test-Path $iisPath) {
        Info "Removing existing sub-app $appPath"
        Remove-WebApplication -Site $UiSite -Name $app.Target -ErrorAction SilentlyContinue
    }
    Step "Creating sub-app $appPath -> $physical"
    New-WebApplication -Site $UiSite -Name $app.Target `
                       -PhysicalPath $physical `
                       -ApplicationPool $UiPool | Out-Null
}

# --- Permissions: IIS_IUSRS needs read on the deployment folders ---------
Step 'Granting IIS_IUSRS read/execute on deployment root'
& icacls $TargetRoot /grant 'IIS_IUSRS:(OI)(CI)RX' /T /Q | Out-Null

# .NET apps need write to their logs subdir
foreach ($p in @($ApiTarget, $SampleTarget)) {
    $logs = Join-Path $p 'logs'
    if (-not (Test-Path $logs)) { New-Item -ItemType Directory -Path $logs -Force | Out-Null }
    & icacls $logs /grant 'IIS_IUSRS:(OI)(CI)M' /T /Q | Out-Null
}

# --- Start everything -----------------------------------------------------
foreach ($p in @($ApiPool, $UiPool, $SamplePool)) {
    Step "Starting pool $p"
    Start-WebAppPool -Name $p
}
foreach ($s in @($ApiSite, $UiSite, $SampleSite)) {
    Step "Starting site $s"
    Start-Website -Name $s
}

Write-Host ''
Write-Host 'Deployed. URLs:' -ForegroundColor Green
Write-Host "  API:         http://localhost:$ApiPort/"        -ForegroundColor Green
Write-Host "  Dashboard:   http://localhost:$UiPort/"         -ForegroundColor Green
foreach ($app in $AngularApps | Where-Object { $_.Target }) {
    Write-Host ("  {0,-12} http://localhost:{1}/{2}/" -f ($app.Name + ':'), $UiPort, $app.Target) -ForegroundColor Green
}
foreach ($port in $SampleAppPorts) {
    Write-Host "  SampleApps:  http://localhost:$port/"       -ForegroundColor Green
}
Write-Host ''
Write-Host 'If a .NET site shows HTTP 500.19 / 500.30, the AspNetCoreModuleV2 hosting' -ForegroundColor Yellow
Write-Host 'bundle is probably missing. Install "ASP.NET Core 8 Hosting Bundle" and restart IIS' -ForegroundColor Yellow
Write-Host '(iisreset).' -ForegroundColor Yellow
