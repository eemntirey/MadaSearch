[CmdletBinding()]
param([string]$AppPath="",[string]$EvidenceRoot=(Join-Path (Get-Location) ("artifacts/recette/"+(Get-Date -Format "yyyyMMdd-HHmmss"))))
$ErrorActionPreference="Continue"
New-Item -ItemType Directory -Force -Path $EvidenceRoot | Out-Null
$prereq=Join-Path $PSScriptRoot "Test-Prerequisites.ps1"
$collect=Join-Path $PSScriptRoot "Collect-Evidence.ps1"
& $prereq -EvidenceRoot $EvidenceRoot
& $collect -EvidenceRoot $EvidenceRoot -AppPath $AppPath
$sha="INCONNU"; try {$sha=(git rev-parse HEAD 2>$null)} catch {}
$rows=1..13 | ForEach-Object { $id=("GUI-{0:D2}" -f $_); "| $id | NON EXÉCUTÉ | Non automatisable intégralement sans application interactive; exécuter la checklist recette-gui.md |" }
$md=@("# Résultats de recette GUI", "", "- Horodatage: $([DateTimeOffset]::Now.ToString('o'))","- SHA du checkout local: $sha","- Binaire: $AppPath","- Statut global: NON EXÉCUTÉ — ce runner collecte les prérequis; il ne simule pas l'interface WPF/WebView2.","","| Scénario | Statut | Motif |","|---|---|---|") + $rows + @("","Aucun PASS n'est attribué automatiquement. Compléter les résultats et preuves dans docs/recette/recette-gui.md.")
$md | Set-Content (Join-Path $EvidenceRoot "recette-resultats.md") -Encoding utf8
Write-Host "Rapport: $(Join-Path $EvidenceRoot 'recette-resultats.md')"
Write-Host "Commande Windows: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/recette/run-all.ps1 -AppPath C:\chemin\MadagascarBrowser.exe -EvidenceRoot artifacts\recette\run-001"