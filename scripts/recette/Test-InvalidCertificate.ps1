[CmdletBinding()]
param([int]$Port=18443)
$ErrorActionPreference="Stop"
$certPath=Join-Path $env:TEMP ("MadaSearch-invalid-"+[guid]::NewGuid().ToString("N")+".cer")
$cert=New-SelfSignedCertificate -DnsName "wrong-host.invalid" -CertStoreLocation "Cert:\CurrentUser\My"
try {
 Export-Certificate -Cert $cert -FilePath $certPath | Out-Null
 Write-Host "Certificat auto-signé généré: $certPath"
 Write-Host "Ce script ne démarre pas de serveur TLS : un hôte local TLS configuré avec ce certificat est requis."
 Write-Host "Ne jamais installer le certificat dans les autorités de confiance. Test manuel attendu sur https://localhost:$Port/."
} finally { Remove-Item -LiteralPath ("Cert:\CurrentUser\My\"+$cert.Thumbprint) -ErrorAction SilentlyContinue }
