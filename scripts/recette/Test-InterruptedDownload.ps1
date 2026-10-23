[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$DatabasePath,[string]$SqliteCli="sqlite3")
$ErrorActionPreference="Stop"
if(!(Test-Path -LiteralPath $DatabasePath)){ throw "Base de téléchargements introuvable: $DatabasePath" }
if(!(Get-Command $SqliteCli -ErrorAction SilentlyContinue)){ throw "sqlite3 CLI absent; inspection non exécutée." }
& $SqliteCli -readonly $DatabasePath ".tables"; & $SqliteCli -readonly $DatabasePath ".schema"
Write-Host "Inspection en lecture seule seulement. Après arrêt contrôlé, interroger la table/colonne réelle pour vérifier Interrupted. Aucun processus n'est tué par ce script."