param([int]$TimeoutSeconds = 90, [switch]$RenderPreview, [int]$PreviewWidth = 1440, [int]$PreviewHeight = 1080)
$ErrorActionPreference = 'Stop'
$gameDir = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (Get-Process -Name valheim -ErrorAction SilentlyContinue) { throw 'Close Valheim before running the isolated tests.' }
& (Join-Path $PSScriptRoot 'build.ps1')
$testDir = Join-Path $PSScriptRoot 'tests\runtime'
New-Item -ItemType Directory -Force -Path $testDir | Out-Null
$log = Join-Path $testDir 'unity.log'
$saves = Join-Path $testDir 'saves'
$report = Join-Path $testDir 'report.txt'
if (Test-Path -LiteralPath $report) { Remove-Item -LiteralPath $report -Force }
$arguments = @('-batchmode', '-nographics', '-vss-self-test', '-vss-test-report', ('"' + $report + '"'), '-savedir', ('"' + $saves + '"'), '-logFile', ('"' + $log + '"'))
if ($RenderPreview) {
    $arguments = $arguments | Where-Object { $_ -ne '-nographics' }
    $previewName = if ($PreviewWidth -eq 1440 -and $PreviewHeight -eq 1080) { 'storage-preview.png' } else { "storage-preview-$($PreviewWidth)x$($PreviewHeight).png" }
    $preview = Join-Path $testDir $previewName
    $arguments += @('-screen-width', $PreviewWidth.ToString(), '-screen-height', $PreviewHeight.ToString(), '-screen-fullscreen', '0', '-vss-preview', ('"' + $preview + '"'))
    $standPreview = Join-Path $testDir 'pedestal-preview.png'
    $arguments += @('-vss-stand-preview', ('"' + $standPreview + '"'))
}
$testProcess = Start-Process -FilePath (Join-Path $gameDir 'valheim.exe') -ArgumentList $arguments -WorkingDirectory $gameDir -WindowStyle Hidden -PassThru
if (-not $testProcess.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $testProcess.Id
    throw "Runtime tests timed out. Inspect $log"
}
$bepLog = Join-Path $gameDir 'BepInEx\LogOutput.log'
Copy-Item -LiteralPath $bepLog -Destination (Join-Path $testDir 'bepinex.log') -Force
$result = if (Test-Path -LiteralPath $report) { Get-Content -LiteralPath $report -Raw } else { 'No test report produced.' }
if ($result -notmatch '^PASS:' -or $testProcess.ExitCode -ne 0) { throw "Runtime test did not pass (exit $($testProcess.ExitCode)): $result" }
$result
