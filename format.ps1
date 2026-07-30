#!/usr/bin/env pwsh
# Applies every auto-fixable formatting/style rule (the .editorconfig set), then
# builds to surface the warnings that dotnet format CANNOT auto-fix so you can
# clean them up by hand (IDE0051/IDE0052 unused private members, etc.).
#
#   ./format.ps1          apply fixes, then report what's left
#   ./format.ps1 -Check   verify only (what CI runs) - no files touched
param(
    [switch]$Check
)

$ErrorActionPreference = 'Stop'
$sln = Join-Path $PSScriptRoot 'Plus.sln'

if ($Check) {
    Write-Host 'Verifying formatting (no changes will be written)...' -ForegroundColor Cyan
    dotnet format $sln --verify-no-changes --verbosity diagnostic
    exit $LASTEXITCODE
}

Write-Host 'Applying dotnet format (whitespace + style + analyzers)...' -ForegroundColor Cyan
dotnet format $sln

Write-Host ''
Write-Host 'Building to list warnings dotnet format cannot auto-fix' -ForegroundColor Yellow
Write-Host '(IDE0051/IDE0052 unused private members need manual removal):' -ForegroundColor Yellow
dotnet build $sln -c Release --nologo -clp:NoSummary
