#!/usr/bin/env pwsh

# Runs the Phase 2 smoke test collection.
# Prereq: dotnet run --project src/ShopSphere.AppHost is running.

$ErrorActionPreference = "Stop"

$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$collectionPath = Join-Path $here ".." "docs" "api" "shopsphere.bru"

Write-Host "Running Bruno smoke collection at $collectionPath"

bru run $collectionPath --env Local --reporter-json smoke-results.json

if ($LASTEXITCODE -ne 0)
{
    Write-Error "Smoke tests failed. See smoke-results.json for details."
    exit $LASTEXITCODE
}

Write-Host "All smoke tests passed." -ForegroundColor Green