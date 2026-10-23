[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ProfilePath,[switch]$ExpectAbsent)
$exists=Test-Path -LiteralPath $ProfilePath
if($ExpectAbsent -and $exists){ Write-Error "FAIL: chemin encore présent: $ProfilePath"; exit 1 }
if($ExpectAbsent -and !$exists){ Write-Host "OBSERVATION: chemin absent; confirmer qu'il s'agit du profil privé réel"; exit 0 }
if($exists){ Get-ChildItem -LiteralPath $ProfilePath -Force -Recurse -ErrorAction SilentlyContinue | Select-Object FullName,Length,LastWriteTime; Write-Host "Chemin présent; vérifier qu'il s'agit du profil privé réel." }
else { Write-Error "Chemin absent avant le test; impossible de vérifier le nettoyage."; exit 2 }