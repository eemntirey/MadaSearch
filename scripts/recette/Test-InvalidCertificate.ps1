[CmdletBinding()]
param([int]$Port=18443)
$ErrorActionPreference="Stop"
$cert=New-SelfSignedCertificate -DnsName "wrong-host.invalid" -CertStoreLocation "Cert:\CurrentUser\My"
$job=$null
try {
  $job=Start-Job -ArgumentList $Port,$cert.Thumbprint -ScriptBlock {
    param($Port,$Thumbprint)
    $ErrorActionPreference="Stop"
    $certificate=Get-Item ("Cert:\CurrentUser\My\"+$Thumbprint)
    $listener=[System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback,$Port)
    $listener.Start()
    try {
      $client=$listener.AcceptTcpClient()
      try {
        $ssl=[System.Net.Security.SslStream]::new($client.GetStream(),$false)
        try {
          $ssl.AuthenticateAsServer($certificate)
          $crlf=[string][char]13+[string][char]10
          $response="HTTP/1.1 200 OK"+$crlf+"Content-Length: 2"+$crlf+"Connection: close"+$crlf+$crlf+"OK"
          $bytes=[System.Text.Encoding]::ASCII.GetBytes($response)
          $ssl.Write($bytes,0,$bytes.Length)
        } finally { $ssl.Dispose() }
      } finally { $client.Dispose() }
    } finally { $listener.Stop() }
  }
  Start-Sleep -Seconds 2
  Write-Host "Serveur TLS local prêt; certificat auto-signé avec nom wrong-host.invalid."
  Write-Host "Naviguez vers https://localhost:$Port/ et observez l'avertissement. Ne contournez pas l'erreur."
  Write-Host "Le serveur accepte une connexion puis s'arrête. Appuyez sur Entrée pour terminer et nettoyer le certificat."
  [void](Read-Host)
} finally {
  if($job){ Stop-Job $job -ErrorAction SilentlyContinue; Receive-Job $job -ErrorAction SilentlyContinue | Out-Null; Remove-Job $job -Force -ErrorAction SilentlyContinue }
  Remove-Item -LiteralPath ("Cert:\CurrentUser\My\"+$cert.Thumbprint) -ErrorAction SilentlyContinue
}
