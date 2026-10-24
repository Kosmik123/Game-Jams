# Uruchamia testy Unity Test Framework w batchmode. Wyniki: Logs\test-<tryb>.xml
#   .\Scripts\test.ps1                    # EditMode
#   .\Scripts\test.ps1 -Platform PlayMode
param(
    [ValidateSet('EditMode', 'PlayMode')]
    [string]$Platform = 'EditMode'
)
. "$PSScriptRoot\common.ps1"

if (Test-ProjectOpen) { throw "Projekt jest otwarty w edytorze Unity - zamknij go przed testami." }

$results = Join-Path $ProjectRoot "Logs\test-$Platform.xml"
Write-Host "==> Testy $Platform" -ForegroundColor Cyan
$r = Invoke-Unity -NoQuit -LogName "test-$Platform" -UnityArgs @(
    '-runTests', '-testPlatform', $Platform, '-testResults', "`"$results`"")

if ($r.ExitCode -ne 0) {
    Show-LogErrors $r.Log
    throw "Testy $Platform nieudane (kod $($r.ExitCode)). Wyniki: $results, log: $($r.Log)"
}
Write-Host "Testy zaliczone. Wyniki: $results" -ForegroundColor Green
