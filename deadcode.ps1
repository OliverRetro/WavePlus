#!/usr/bin/env pwsh
# Solution-wide dead-code report via JetBrains ReSharper CLI (InspectCode).
# Finds what Roslyn's IDE0051/IDE0052 cannot: unused CLASSES / whole files and
# unused PUBLIC members, using cross-project reference analysis.
#
# Report-only. Nothing is deleted. Treat hits as a review list - this server resolves
# packet handlers / DI by name, and EF materializes entity properties by reflection, so
# a "never used" symbol is often a false positive. Confirm each before removing.
#
#   ./deadcode.ps1          run inspection + print the report
#   ./deadcode.ps1 -Reuse   skip the (slow) inspection, re-print from the last inspect.sarif
param(
    [switch]$Reuse
)

$ErrorActionPreference = 'Stop'
$sln = Join-Path $PSScriptRoot 'Plus.sln'
$out = Join-Path $PSScriptRoot 'inspect.sarif'

if (-not $Reuse) {
    if (-not (Get-Command jb -ErrorAction SilentlyContinue)) {
        Write-Host 'Installing JetBrains ReSharper CLI (one-time)...' -ForegroundColor Cyan
        dotnet tool install --global JetBrains.ReSharper.GlobalTools | Out-Null
        $env:PATH += ";$env:USERPROFILE\.dotnet\tools"
    }
    Write-Host 'Running solution-wide inspection (a few minutes; per-file spam suppressed)...' -ForegroundColor Cyan
    # --no-swea=false keeps solution-wide analysis on (needed for the *.Global unused rules).
    # --exclude skips the EF entity DTOs: their properties are only ever read by EF via
    #   reflection, so every one shows up as a false-positive "unused property" otherwise.
    # jb prints one "Analyzing X.cs" line per file - dump that to a log, not the console.
    # jb writes MSBuild warnings to stderr; under $ErrorActionPreference='Stop' PowerShell
    # would turn those into a terminating error, so relax it just for this native call.
    $prev = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    jb inspectcode $sln --output=$out --severity=WARNING --no-swea=false --exclude="WavePlus.Entities/**;Database/EF/**" 2>&1 |
        Out-File -FilePath (Join-Path $PSScriptRoot 'inspect.log') -Encoding utf8
    $ErrorActionPreference = $prev
}

if (-not (Test-Path $out)) { throw "No report at $out - run without -Reuse first." }

$results = (Get-Content $out -Raw | ConvertFrom-Json).runs[0].results
function Loc($r) {
    $p = $r.locations[0].physicalLocation
    '{0}:{1}' -f [Uri]::UnescapeDataString($p.artifactLocation.uri), $p.region.startLine
}

$cats = [ordered]@{
    'UNUSED CLASSES / WHOLE FILES'  = @('UnusedType.Global')
    'UNUSED PUBLIC MEMBERS/METHODS' = @('UnusedMember.Global', 'UnusedMemberInSuper.Global', 'UnusedMethodReturnValue.Global')
    'DEAD PRIVATE FIELDS/MEMBERS'   = @('IDE0052', 'IDE0051', 'NotAccessedField.Local', 'NotAccessedField.Global', 'UnusedMember.Local', 'PrivateFieldCanBeConvertedToLocalVariable', 'UnusedMethodReturnValue.Local')
    'UNUSED PROPERTIES'             = @('UnusedAutoPropertyAccessor.Global', 'UnusedAutoPropertyAccessor.Local')
    'DEAD LOCALS / PARAMETERS'      = @('UnusedVariable', 'UnusedParameter.Local', 'RedundantAssignment')
}

Write-Host ''
foreach ($c in $cats.Keys) {
    $hits = $results | Where-Object { $cats[$c] -contains $_.ruleId }
    Write-Host ("=== {0} ({1}) ===" -f $c, @($hits).Count) -ForegroundColor Yellow
    $hits | Sort-Object { Loc $_ } | ForEach-Object { '  {0,-42} {1}  {2}' -f $_.ruleId, (Loc $_), $_.message.text }
    Write-Host ''
}
Write-Host "Full SARIF report: $out (open in VS / any SARIF viewer for the other ~950 style hits)" -ForegroundColor DarkGray
