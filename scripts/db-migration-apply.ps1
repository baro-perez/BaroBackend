<#
.SYNOPSIS
  Adds an EF Core migration to BaroBackend.Infrastructure.
.EXAMPLE
  ./scripts/db-migration.ps1 AddHeroCollection
#>
param(
    [Parameter(Position = 0)]
    [string]$TargetMigration)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$infrastructure = Join-Path $root "src/BaroBackend.Infrastructure"
$api = Join-Path $root "src/BaroBackend.Api"

$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$env:ASPNETCORE_ENVIRONMENT = "Development"
try {

    $arguments = @("ef", "database", "update")
    if (-not [string]::IsNullOrWhiteSpace($TargetMigration)) {
        $arguments += $TargetMigration
    }
	else {
		dotnet ef migrations list --project $infrastructure --startup-project $api
	}
    $arguments += @("--project", $infrastructure, "--startup-project", $api)
	
    dotnet @arguments

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet ef failed (exit code $LASTEXITCODE)."
    }

    Write-Host "Migration(s) applied"

}
finally {
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
}