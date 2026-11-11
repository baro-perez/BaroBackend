<#
.SYNOPSIS
  Adds an EF Core migration to BaroBackend.Infrastructure.
.EXAMPLE
  ./scripts/db-migration.ps1 AddHeroCollection
#>
param(
    [Parameter(Position = 0)]
    [string]$Name)

$ErrorActionPreference = "Stop"

if ($Name -notmatch '^[A-Z][A-Za-z0-9]*$') {
    Write-Host "Migration name must be PascalCase (letters and digits only): '$Name'"
    exit 1
}

if ([string]::IsNullOrWhiteSpace($Name)) {
    Write-Host "Usage:   ./scripts/db-migration.ps1 <MigrationName>"
    Write-Host "Example: ./scripts/db-migration.ps1 AddHeroCollection"
    exit 1
}
  
$root = Split-Path -Parent $PSScriptRoot
$infrastructure = Join-Path $root "src/BaroBackend.Infrastructure"
$api = Join-Path $root "src/BaroBackend.Api"

$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$env:ASPNETCORE_ENVIRONMENT = "Development"
try {
    dotnet ef migrations add $Name `
        --project $infrastructure `
        --startup-project $api `
        --output-dir Persistence/Migrations

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet ef failed (exit code $LASTEXITCODE)."
    }

    Write-Host "Migration '$Name' created. Review the generated SQL before applying it."
    Write-Host "Undo with ./scripts/db-migration-undo.ps1"

}
finally {
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
}