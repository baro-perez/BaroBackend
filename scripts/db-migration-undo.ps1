<#
.SYNOPSIS
  Remove the last EF Core migration into BaroBackend.Infrastructure.
.EXAMPLE
  ./scripts/db-migration-undo.ps1
#>
  
$root = Split-Path -Parent $PSScriptRoot
$infrastructure = Join-Path $root "src/BaroBackend.Infrastructure"
$api = Join-Path $root "src/BaroBackend.Api"

$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$env:ASPNETCORE_ENVIRONMENT = "Development"
try {
    dotnet ef migrations remove `
        --project $infrastructure `
        --startup-project $api

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet ef failed (exit code $LASTEXITCODE)."
    }
}
finally {
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
}