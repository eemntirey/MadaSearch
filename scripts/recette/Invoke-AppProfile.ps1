[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$AppPath,[string]$ProfilePath=(Join-Path $env:TEMP ("MadaSearch-Recette-"+[guid]::NewGuid().ToString("N"))),[string]$Arguments="")
$ErrorActionPreference="Stop"
if(!(Test-Path -LiteralPath $AppPath -PathType Leaf)){ throw "Binaire introuvable: $AppPath" }
New-Item -ItemType Directory -Force -Path $ProfilePath | Out-Null
$hash=(Get-FileHash -LiteralPath $AppPath -Algorithm SHA256).Hash
Write-Host "Executable SHA256: $hash"
Write-Host "Dossier isolé préparé: $ProfilePath"
Write-Host "ATTENTION: ce dossier n'est pas le profil applicatif effectif sauf si l'application accepte explicitement un argument de profil."
$argList=@(); if($Arguments){$argList += $Arguments}
$p=Start-Process -FilePath $AppPath -ArgumentList $argList -PassThru
[pscustomobject]@{Pid=$p.Id;Path=$AppPath;Sha256=$hash;ProfilePath=$ProfilePath;StartedAt=[DateTimeOffset]::Now.ToString("o")} | ConvertTo-Json | Set-Content (Join-Path $ProfilePath "recette-launch.json")
Write-Host "PID=$($p.Id). Fermez normalement pour les tests de nettoyage ou terminez le processus seulement pour un scénario crash dédié."