[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$DatabasePath,[string]$SqliteCli="sqlite3")
$ErrorActionPreference="Stop"
if(!(Test-Path -LiteralPath $DatabasePath)){ throw "Base SQLite introuvable: $DatabasePath" }
if(!(Get-Command $SqliteCli -ErrorAction SilentlyContinue)){ throw "sqlite3 CLI absent. Aucun changement de données n'a été effectué." }
& $SqliteCli -readonly $DatabasePath ".tables"; if($LASTEXITCODE -ne 0){throw "Échec de lecture SQLite"}
& $SqliteCli -readonly $DatabasePath ".schema"; if($LASTEXITCODE -ne 0){throw "Échec du schéma SQLite"}
Write-Host "Lecture seule. Identifier le vrai schéma, puis comparer les catégories avant/après; aucune suppression n'est réalisée par ce script."