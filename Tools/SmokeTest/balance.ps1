# Telemetria balansu: bot gra pelne runy i zapisuje CSV.
# Uzycie: powershell -File Tools/SmokeTest/balance.ps1 -Strategy careful -Runs 10 [-Speed 8] [-Asc 0] [-Out C:\sciezka\out.csv]
param(
    [string]$Strategy = "careful",   # random | careful | buyall
    [int]$Runs = 5,
    [float]$Speed = 8,
    [int]$Asc = 0,
    [string]$Force = "",
    [string]$Out = "$env:TEMP\jam_bot_$Strategy.csv"
)
$repo = Resolve-Path "$PSScriptRoot\..\.."
$dst = "$env:TEMP\jam_bot_$Strategy"
New-Item -ItemType Directory -Force $dst | Out-Null
foreach ($d in 'Assets','Packages','ProjectSettings') { robocopy "$repo\$d" "$dst\$d" /E /NFL /NDL /NJH /NJS /NP | Out-Null }
New-Item -ItemType Directory -Force "$dst\Assets\SmokeTests" | Out-Null
Copy-Item "$PSScriptRoot\BalanceBot.cs","$PSScriptRoot\SmokeTests.asmdef" "$dst\Assets\SmokeTests" -Force
$env:BOT_STRATEGY = $Strategy; $env:BOT_RUNS = "$Runs"; $env:BOT_SPEED = "$Speed"; $env:BOT_ASC = "$Asc"; $env:BOT_FORCE = $Force; $env:BOT_OUT = $Out
$unity = "C:\Program Files\Unity\Hub\Editor\6000.3.27f1\Editor\Unity.exe"
$p = Start-Process $unity -ArgumentList @('-batchmode','-noaudio','-projectPath',"`"$dst`"",'-runTests','-testPlatform','PlayMode','-testFilter','BalanceBot','-testResults',"`"$dst\results.xml`"",'-logFile',"`"$dst\unity.log`"") -PassThru -Wait -NoNewWindow
"exit $($p.ExitCode); csv: $Out"
Select-String -Path "$dst\unity.log" -Pattern '\[bot\]' | % Line
