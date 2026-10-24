$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repositoryRoot
try {
    dotnet build MadagascarBrowser.sln --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "Build en échec : la recette E2E ne peut pas démarrer."
    }

    dotnet test tests/MadagascarBrowser.E2E/MadagascarBrowser.E2E.csproj --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "Recette E2E en échec."
    }

    Write-Host "Recette E2E réussie. Captures : docs/captures/e2e"
}
finally {
    Pop-Location
}
