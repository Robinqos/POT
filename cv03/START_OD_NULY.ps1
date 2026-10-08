param([Parameter(Mandatory=$true)][string]$Destination)
$ErrorActionPreference = 'Stop'
$sourceRoot = $PSScriptRoot
$newWorkspace = [System.IO.Path]::GetFullPath($Destination)
if (Test-Path -LiteralPath $newWorkspace) { throw 'Vyberte nový, ešte neexistujúci priečinok.' }

function Invoke-Dotnet([string[]]$Arguments) {
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw ('dotnet zlyhal: ' + ($Arguments -join ' ')) }
}

New-Item -ItemType Directory -Path $newWorkspace | Out-Null
Invoke-Dotnet -Arguments @('new','sln','--name','PlaylistLab','--format','slnx','--output',$newWorkspace)
Invoke-Dotnet -Arguments @('new','classlib','--name','PlaylistLab.Core','--framework','net10.0','--no-restore','--output',(Join-Path $newWorkspace 'PlaylistLab.Core'))
Invoke-Dotnet -Arguments @('new','console','--name','PlaylistLab.App','--framework','net10.0','--no-restore','--output',(Join-Path $newWorkspace 'PlaylistLab.App'))

$generatedClass = [System.IO.Path]::GetFullPath((Join-Path $newWorkspace 'PlaylistLab.Core\Class1.cs'))
if (-not $generatedClass.StartsWith($newWorkspace + [System.IO.Path]::DirectorySeparatorChar,[System.StringComparison]::OrdinalIgnoreCase)) { throw 'Neplatná cesta.' }
if (Test-Path -LiteralPath $generatedClass) { Remove-Item -LiteralPath $generatedClass }

foreach ($directory in @('Collections','Models','Services')) {
    Copy-Item -LiteralPath (Join-Path $sourceRoot ('PlaylistLab.Core\' + $directory)) -Destination (Join-Path $newWorkspace 'PlaylistLab.Core') -Recurse
}
foreach ($relative in @('PlaylistLab.Core\PlaylistLab.Core.csproj','PlaylistLab.Core\PACKAGE_README.md','PlaylistLab.App\PlaylistLab.App.csproj')) {
    Copy-Item -LiteralPath (Join-Path $sourceRoot $relative) -Destination (Join-Path $newWorkspace $relative)
}
Get-ChildItem -LiteralPath (Join-Path $sourceRoot 'PlaylistLab.App') -Filter '*.cs' -File | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $newWorkspace 'PlaylistLab.App')
}
$testDirectory = Join-Path $newWorkspace 'PlaylistLab.Tests'
New-Item -ItemType Directory -Path $testDirectory | Out-Null
foreach ($filename in @('PlaylistLab.Tests.csproj','PlaylistTests.cs')) {
    Copy-Item -LiteralPath (Join-Path $sourceRoot ('PlaylistLab.Tests\' + $filename)) -Destination (Join-Path $testDirectory $filename)
}
$solutionPath = Join-Path $newWorkspace 'PlaylistLab.slnx'
foreach ($project in @('PlaylistLab.Core','PlaylistLab.App','PlaylistLab.Tests')) {
    Invoke-Dotnet -Arguments @('sln',$solutionPath,'add',(Join-Path $newWorkspace ($project + '\' + $project + '.csproj')))
}
Invoke-Dotnet -Arguments @('restore',$solutionPath)
Invoke-Dotnet -Arguments @('build',$solutionPath,'--no-restore')
Write-Output ('Vytvorené vlastné projekty: ' + $solutionPath)
