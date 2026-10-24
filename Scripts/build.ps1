# Build gry przez Unity CLI. Przyklady:
#   .\Scripts\build.ps1                       # Windows + WebGL
#   .\Scripts\build.ps1 -Targets Windows
param(
    [ValidateSet('Windows', 'Linux', 'WebGL')]
    [string[]]$Targets = @('Windows', 'WebGL')
)
. "$PSScriptRoot\common.ps1"

if (Test-ProjectOpen) { throw "Projekt jest otwarty w edytorze Unity - zamknij go przed buildem." }

foreach ($t in $Targets) {
    Write-Host "==> Build $t" -ForegroundColor Cyan
    $r = Invoke-Unity -UnityArgs @('-executeMethod', "BuildScript.$t") -LogName "build-$t"
    if ($r.ExitCode -ne 0) {
        Show-LogErrors $r.Log
        throw "Build $t nieudany (kod $($r.ExitCode)). Pelny log: $($r.Log)"
    }
}
Write-Host "Gotowe. Wyniki: $ProjectRoot\Builds" -ForegroundColor Green
