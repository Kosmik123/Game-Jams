# Build + publikacja na itch.io przez butler. Przyklady:
#   .\Scripts\deploy.ps1 -DryRun                 # nic nie wysyla, sprawdza polaczenie
#   .\Scripts\deploy.ps1 -Version v0.1.0
#   .\Scripts\deploy.ps1 -SkipBuild -Targets WebGL   # wyslij istniejacy build
param(
    [ValidateSet('Windows', 'Linux', 'WebGL')]
    [string[]]$Targets = @('Windows', 'WebGL'),
    [string]$Version,
    [switch]$SkipBuild,
    [switch]$DryRun
)
. "$PSScriptRoot\common.ps1"

if (-not $ItchTarget -or $ItchTarget -like 'twoj-login*') {
    throw "Ustaw `$ItchTarget (uzytkownik/gra) w Scripts/config.ps1."
}
if (-not (Get-Command butler -ErrorAction SilentlyContinue)) {
    throw "butler nie jest w PATH. Zobacz docs/SETUP.md (sekcja Butler)."
}

if (-not $Version) { $Version = (git -C $ProjectRoot describe --tags --always 2>$null) }
if (-not $Version) { $Version = Get-Date -Format 'yyyy.MM.dd-HHmm' }

if (git -C $ProjectRoot status --porcelain 2>$null) {
    Write-Warning "Masz niezacommitowane zmiany - wersja '$Version' moze nie odpowiadac kodowi w buildzie."
}

if (-not $SkipBuild) { & "$PSScriptRoot\build.ps1" -Targets $Targets }

# Nazwy kanalow: itch.io rozpoznaje platforme po nazwie (windows/linux). WebGL oznacz recznie na stronie gry.
$channels = @{ Windows = 'windows'; Linux = 'linux'; WebGL = 'webgl' }

foreach ($t in $Targets) {
    $dir = Join-Path $ProjectRoot "Builds\$($t.ToLower())"
    if (-not (Test-Path $dir)) { throw "Brak buildu: $dir (uruchom bez -SkipBuild)." }

    $butlerArgs = @('push', $dir, "${ItchTarget}:$($channels[$t])", '--userversion', $Version)
    if ($DryRun) { $butlerArgs += '--dry-run' }

    Write-Host "==> butler $($butlerArgs -join ' ')" -ForegroundColor Cyan
    & butler @butlerArgs
    if ($LASTEXITCODE -ne 0) { throw "butler push ($t) nieudany (kod $LASTEXITCODE)." }
}
Write-Host "Gotowe: $ItchTarget @ $Version" -ForegroundColor Green
