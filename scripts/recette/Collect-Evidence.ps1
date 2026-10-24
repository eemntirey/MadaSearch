[CmdletBinding()]
param([string]$EvidenceRoot=(Join-Path (Get-Location) ("artifacts/recette/"+(Get-Date -Format "yyyyMMdd-HHmmss"))),[string]$AppPath="")
$ErrorActionPreference="Continue"
New-Item -ItemType Directory -Force -Path $EvidenceRoot | Out-Null
Start-Transcript -Path (Join-Path $EvidenceRoot "collect-transcript.txt") -Force | Out-Null
try {
 "Timestamp: $([DateTimeOffset]::Now.ToString('o'))"
 "Computer: $env:COMPUTERNAME"; "User: $env:USERNAME"
 Get-CimInstance Win32_OperatingSystem | Select-Object Caption,Version,BuildNumber,OSArchitecture
 dotnet --list-runtimes 2>&1
 if($AppPath -and (Test-Path -LiteralPath $AppPath)){ Get-FileHash -LiteralPath $AppPath -Algorithm SHA256 }
 "Captures: utilisez Win+Shift+S; sauvegardez en PNG dans $EvidenceRoot avec l'ID GUI-xx dans le nom."
} finally { Stop-Transcript | Out-Null }
Write-Host "Preuves textuelles dans $EvidenceRoot. Les captures doivent être prises par l'opérateur."