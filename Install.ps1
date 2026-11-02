param([string]$GameRoot = (Join-Path $PSScriptRoot '..\..'), [switch]$StageForRestart)
$ErrorActionPreference = 'Stop'
$GameRoot = (Resolve-Path -LiteralPath $GameRoot).Path
if (!(Test-Path -LiteralPath (Join-Path $GameRoot 'KingdomInc.exe'))) { throw 'Choose the KingdomInc game directory.' }
$gameRunning = [bool](Get-Process -Name KingdomInc -ErrorAction SilentlyContinue)
if ($gameRunning -and !$StageForRestart) { throw 'Close Kingdom .inc before installing, or use -StageForRestart to update only the on-disk DLL for the next launch.' }
$source = Join-Path $PSScriptRoot 'bin\Release\netstandard2.1\SharableBlueprints.dll'
if (!(Test-Path -LiteralPath $source)) { throw 'Build the Release project first.' }
$destination = Join-Path $GameRoot 'BepInEx\plugins\SharableBlueprints'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$target = Join-Path $destination 'SharableBlueprints.dll'
if (Test-Path -LiteralPath $target) {
    $backup = Join-Path $PSScriptRoot ('_Backup\' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
    New-Item -ItemType Directory -Path $backup -Force | Out-Null
    Copy-Item -LiteralPath $target -Destination $backup
}
$staged = Join-Path $destination ('SharableBlueprints.' + [Guid]::NewGuid().ToString('N') + '.pending')
try {
    Copy-Item -LiteralPath $source -Destination $staged
    if (Test-Path -LiteralPath $target) { [System.IO.File]::Replace($staged, $target, (Join-Path $backup 'SharableBlueprints-previous.dll')) }
    else { [System.IO.File]::Move($staged, $target) }
} finally {
    if (Test-Path -LiteralPath $staged) { Remove-Item -LiteralPath $staged }
}
if ((Get-FileHash -LiteralPath $source).Hash -ne (Get-FileHash -LiteralPath $target).Hash) { throw 'Installed DLL verification failed.' }
Write-Output "Installed Sharable Blueprints: $target"
if ($gameRunning) { Write-Output 'The running game keeps its loaded version. Restart the game to load this update.' }
