# Wspólne funkcje dla build.ps1, test.ps1 i deploy.ps1 (dot-sourcing: . .\common.ps1)
$ErrorActionPreference = 'Stop'
$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

$configPath = Join-Path $PSScriptRoot 'config.ps1'
if (Test-Path $configPath) { . $configPath }
else { Write-Warning "Brak Scripts/config.ps1 - skopiuj config.example.ps1 i uzupelnij." }

function Get-UnityExe {
    if ($UnityExe -and (Test-Path $UnityExe)) { return $UnityExe }

    $versionFile = Join-Path $ProjectRoot 'ProjectSettings\ProjectVersion.txt'
    if (-not (Test-Path $versionFile)) { throw "Brak $versionFile - to nie jest projekt Unity?" }
    $ver = (Select-String -Path $versionFile -Pattern 'm_EditorVersion:\s*(\S+)').Matches[0].Groups[1].Value

    $hub = if ($UnityHubEditorsDir) { $UnityHubEditorsDir } else { 'C:\Program Files\Unity\Hub\Editor' }
    $exe = Join-Path $hub "$ver\Editor\Unity.exe"
    if (-not (Test-Path $exe)) {
        throw "Nie znaleziono Unity $ver pod $exe. Zainstaluj te wersje w Unity Hub albo ustaw `$UnityExe w Scripts/config.ps1."
    }
    return $exe
}

# True, jesli projekt jest otwarty w edytorze (Unity trzyma blokade na Temp\UnityLockfile).
function Test-ProjectOpen {
    $lock = Join-Path $ProjectRoot 'Temp\UnityLockfile'
    if (-not (Test-Path $lock)) { return $false }
    try {
        $s = [IO.File]::Open($lock, 'Open', 'ReadWrite', 'None')
        $s.Close()
        return $false
    } catch { return $true }
}

# Uruchamia Unity w batchmode i CZEKA na zakonczenie (Unity.exe to aplikacja GUI,
# wiec zwykle "& Unity.exe" nie czeka - dlatego Start-Process -Wait).
function Invoke-Unity {
    param(
        [string[]]$UnityArgs,
        [string]$LogName,
        [switch]$NoQuit   # dla -runTests Unity zamyka sie samo
    )
    $exe = Get-UnityExe
    $log = Join-Path $ProjectRoot "Logs\$LogName.log"
    New-Item -ItemType Directory -Force (Split-Path $log) | Out-Null

    $argList = @('-batchmode', '-projectPath', "`"$ProjectRoot`"", '-logFile', "`"$log`"")
    if (-not $NoQuit) { $argList += '-quit' }
    $argList += $UnityArgs

    $p = Start-Process -FilePath $exe -ArgumentList $argList -Wait -PassThru
    return [pscustomobject]@{ ExitCode = $p.ExitCode; Log = $log }
}

# Wypisuje ostatnie linie z bledami z logu Unity (przydatne dla czlowieka i dla Claude Code).
function Show-LogErrors {
    param([string]$Log)
    if (-not (Test-Path $Log)) { return }
    Select-String -Path $Log -Pattern 'error CS\d+|BuildFailedException|Exception:|\[BuildScript\]' |
        Select-Object -Last 15 | ForEach-Object { Write-Host $_.Line -ForegroundColor Red }
}
