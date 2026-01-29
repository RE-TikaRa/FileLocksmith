param(
    [ValidateSet('x64','ARM64')]
    [string]$Platform = 'x64',
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release',
    [ValidateSet('Portable','System')]
    [string]$Mode = 'Portable',
    [bool]$IncludeVCLibs = $true
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$winuiFolderName = if ($Mode -eq 'Portable') { 'WinUI3Apps' } else { 'WinUI3Apps.System' }
$winuiDir = Join-Path -Path $repoRoot -ChildPath (Join-Path $Platform (Join-Path $Configuration $winuiFolderName))
$binDir = Join-Path -Path $repoRoot -ChildPath (Join-Path $Platform $Configuration)
$outName = if ($Mode -eq 'Portable') { 'FileLocksmith Portable version' } else { 'FileLocksmith System dependency versions' }
$outDir = Join-Path -Path $repoRoot -ChildPath (Join-Path 'artifacts' (Join-Path $outName (Join-Path $Platform $Configuration)))

if (-not $PSBoundParameters.ContainsKey('IncludeVCLibs'))
{
    $IncludeVCLibs = ($Mode -eq 'Portable')
}

Write-Host ('Pack mode: {0}' -f $Mode)

if (!(Test-Path $winuiDir))
{
    throw ('Build output not found: {0}. Build File Locksmith first.' -f $winuiDir)
}

New-Item -ItemType Directory -Path $outDir -Force | Out-Null
Get-ChildItem -Path $outDir -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

function Copy-IfExists {
    param(
        [string]$Path,
        [string]$Destination
    )
    if (Test-Path $Path)
    {
        Copy-Item -Path $Path -Destination $Destination -Force
        return $true
    }
    return $false
}

function Copy-IfNotSame {
    param(
        [string]$Path,
        [string]$Destination
    )
    if (!(Test-Path $Path))
    {
        return $false
    }
    $destPath = $Destination
    if (Test-Path $Destination -PathType Container)
    {
        $destPath = Join-Path $Destination (Split-Path $Path -Leaf)
    }
    $sourceFull = [System.IO.Path]::GetFullPath($Path)
    $destFull = [System.IO.Path]::GetFullPath($destPath)
    if ($sourceFull -ieq $destFull)
    {
        return $false
    }
    Copy-Item -Path $Path -Destination $Destination -Force
    return $true
}

function Copy-Glob {
    param(
        [string]$Pattern,
        [string]$Destination,
        [switch]$Recurse
    )
    $items = Get-ChildItem -Path $Pattern -ErrorAction SilentlyContinue
    if ($items)
    {
        if ($Recurse)
        {
            Copy-Item -Path $items.FullName -Destination $Destination -Recurse -Force
        }
        else
        {
            Copy-Item -Path $items.FullName -Destination $Destination -Force
        }
        return $true
    }
    return $false
}

function Find-First {
    param(
        [string]$Root,
        [string]$Filter
    )
    $item = Get-ChildItem -Path $Root -Filter $Filter -Recurse -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($item)
    {
        return $item.FullName
    }
    return $null
}

Write-Host 'Copy UI output...'
Copy-Item -Path (Join-Path $winuiDir '*') -Destination $outDir -Recurse -Force

Write-Host 'Copy CLI...'
if (-not (Copy-IfExists (Join-Path $binDir 'FileLocksmithCLI.exe') $outDir))
{
    Write-Warning 'FileLocksmithCLI.exe not found.'
}
Copy-IfExists (Join-Path $binDir 'FileLocksmithCLI.pdb') $outDir | Out-Null

Write-Host 'Copy shell extension...'
$extPath = Join-Path $binDir 'PowerToys.FileLocksmithExt.dll'
if (!(Test-Path $extPath))
{
    $extPath = Join-Path $binDir 'FileLocksmithExt.dll'
}
if (!(Test-Path $extPath))
{
    $extPath = Find-First $repoRoot 'PowerToys.FileLocksmithExt.dll'
}
if ($extPath)
{
    Copy-IfNotSame -Path $extPath -Destination $outDir | Out-Null
}
else
{
    Write-Warning 'PowerToys.FileLocksmithExt.dll not found.'
}

Write-Host 'Copy context menu package...'
$msixPath = Find-First $repoRoot 'FileLocksmithContextMenuPackage*.msix'
if ($msixPath)
{
    Copy-IfNotSame -Path $msixPath -Destination $outDir | Out-Null
}
else
{
    Write-Warning 'FileLocksmithContextMenuPackage.msix not found.'
}

Write-Host 'Copy extra tools...'
Copy-IfExists (Join-Path $binDir 'PowerToys.Interop.dll') $outDir | Out-Null
Copy-IfExists (Join-Path $binDir 'PowerToys.Interop.winmd') $outDir | Out-Null
Copy-IfExists (Join-Path $PSScriptRoot 'register.ps1') $outDir | Out-Null
Copy-IfExists (Join-Path $PSScriptRoot 'unregister.ps1') $outDir | Out-Null
Copy-IfExists (Join-Path $PSScriptRoot 'launch.cmd') $outDir | Out-Null

if ($IncludeVCLibs)
{
    Write-Host 'Prepare VCLibs...'
    $vclibsDir = Join-Path $outDir 'VCLibs'
    New-Item -ItemType Directory -Path $vclibsDir -Force | Out-Null
    $arch = if ($Platform -eq 'ARM64') { 'arm64' } else { 'x64' }
    $vclibsName = "Microsoft.VCLibs.$arch.14.00.Desktop.appx"
    $targetVclibs = Join-Path $vclibsDir $vclibsName
    if (!(Test-Path $targetVclibs))
    {
        $existing = Find-First $repoRoot $vclibsName
        if ($existing)
        {
            Copy-Item -Path $existing -Destination $targetVclibs -Force
        }
        else
        {
            try
            {
                $url = "https://aka.ms/Microsoft.VCLibs.$arch.14.00.Desktop.appx"
                Invoke-WebRequest -Uri $url -OutFile $targetVclibs
            }
            catch
            {
                Write-Warning ("VCLibs download failed: {0}" -f $_.Exception.Message)
            }
        }
    }
}

Write-Host ('Packed to: {0}' -f $outDir)
