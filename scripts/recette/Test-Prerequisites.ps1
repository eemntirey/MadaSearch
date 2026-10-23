[CmdletBinding()]
param([string]$EvidenceRoot = (Join-Path (Get-Location) "artifacts/recette"))
$ErrorActionPreference = "Continue"
New-Item -ItemType Directory -Force -Path $EvidenceRoot | Out-Null
$report = Join-Path $EvidenceRoot "prerequis.txt"
"Timestamp: $([DateTimeOffset]::Now.ToString('o'))" | Set-Content $report
"OS:" | Add-Content $report
Get-CimInstance Win32_OperatingSystem | Select-Object Caption,Version,BuildNumber,OSArchitecture | Format-List | Out-String | Add-Content $report
".NET runtimes:" | Add-Content $report
if (Get-Command dotnet -ErrorAction SilentlyContinue) { dotnet --list-runtimes 2>&1 | Add-Content $report } else { "dotnet CLI not found" | Add-Content $report }
"WebView2 registry:" | Add-Content $report
$roots = @("HKLM:\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients","HKLM:\SOFTWARE\Microsoft\EdgeUpdate\Clients","HKCU:\SOFTWARE\Microsoft\EdgeUpdate\Clients")
foreach($root in $roots) { if(Test-Path $root) { Get-ChildItem $root -ErrorAction SilentlyContinue | ForEach-Object { $p=Get-ItemProperty $_.PSPath -ErrorAction SilentlyContinue; if($p.name -match "WebView2|Microsoft Edge WebView2 Runtime") { [pscustomobject]@{Path=$_.Name;Name=$p.name;Version=$p.pv} | Format-List | Out-String | Add-Content $report } } } }
Get-Content $report