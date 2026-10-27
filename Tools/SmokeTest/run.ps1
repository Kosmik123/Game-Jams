# Smoke test: kopiuje projekt do katalogu tymczasowego, dodaje test PlayMode i przechodzi caly run (3 strefy) z losowymi zakupami.
# Uzycie: powershell -File Tools/SmokeTest/run.ps1   (Unity 6000.3.27f1, wymaga GPU - bez -nographics)
$repo = Resolve-Path "$PSScriptRoot\..\.."
$dst = "$env:TEMP\jam_smoketest"
New-Item -ItemType Directory -Force $dst | Out-Null
foreach ($d in 'Assets','Packages','ProjectSettings') { robocopy "$repo\$d" "$dst\$d" /E /NFL /NDL /NJH /NJS /NP | Out-Null }
New-Item -ItemType Directory -Force "$dst\Assets\SmokeTests" | Out-Null
Copy-Item "$PSScriptRoot\SmokeTest.cs","$PSScriptRoot\SmokeTests.asmdef" "$dst\Assets\SmokeTests" -Force
$unity = "C:\Program Files\Unity\Hub\Editor\6000.3.27f1\Editor\Unity.exe"
$p = Start-Process $unity -ArgumentList @('-batchmode','-projectPath',"`"$dst`"",'-runTests','-testPlatform','PlayMode','-testResults',"`"$dst\results.xml`"",'-logFile',"`"$dst\unity.log`"") -PassThru -Wait -NoNewWindow
"exit $($p.ExitCode) (0 = OK)"
Select-String -Path "$dst\unity.log" -Pattern '\[smoke\]|NullReference|Exception:' | Select -Last 25 | % Line
