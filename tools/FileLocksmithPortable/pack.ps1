param(
    [ValidateSet('x64','ARM64')]
    [string]$Platform = 'x64',
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release',
    [switch]$IncludeVCLibs = $true
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$winuiDir = Join-Path -Path $repoRoot -ChildPath (Join-Path $Platform (Join-Path $Configuration 'WinUI3Apps'))
$binDir = Join-Path -Path $repoRoot -ChildPath (Join-Path $Platform $Configuration)
$outDir = Join-Path -Path $repoRoot -ChildPath (Join-Path 'artifacts\FileLocksmithPortable' (Join-Path $Platform $Configuration))

if (!(Test-Path $winuiDir))
{
    throw ('未找到构建输出：{0}。请先构建 File Locksmith 相关项目。' -f $winuiDir)
}

New-Item -ItemType Directory -Path $outDir -Force | Out-Null

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

function Copy-Glob {
    param(
        [string]$SourceDir,
        [string]$Pattern
    )
    Get-ChildItem -Path $SourceDir -Filter $Pattern -File -ErrorAction SilentlyContinue |
        ForEach-Object { Copy-Item -Path $_.FullName -Destination $outDir -Force }
}

# UI 主程序与依赖
Copy-IfExists (Join-Path $winuiDir 'PowerToys.FileLocksmithUI.exe') $outDir | Out-Null
Copy-Glob $winuiDir 'PowerToys.FileLocksmithUI.*'
Copy-Glob $winuiDir '*.pri'
Copy-Glob $winuiDir '*.dll'
Copy-Glob $winuiDir '*.json'

# 运行时资源文件夹（XAML/MUI/WinUI 资源等）
Get-ChildItem -Path $winuiDir -Directory -ErrorAction SilentlyContinue |
    ForEach-Object { Copy-Item -Path $_.FullName -Destination (Join-Path $outDir $_.Name) -Recurse -Force }

# 右键菜单包
Copy-IfExists (Join-Path $winuiDir 'FileLocksmithContextMenuPackage.msix') $outDir | Out-Null
Copy-Glob $winuiDir 'PowerToys.FileLocksmithContextMenu*'
Copy-Glob $winuiDir 'PowerToys.FileLocksmithExt*'
Copy-Glob $winuiDir 'PowerToys.FileLocksmithLib.Interop*'

# CLI
$cliPath = Join-Path $binDir 'FileLocksmithCLI.exe'
Copy-IfExists $cliPath $outDir | Out-Null

# 资源
$assetsSrc = Join-Path $winuiDir 'Assets\FileLocksmith'
$assetsDst = Join-Path $outDir 'Assets\FileLocksmith'
if (Test-Path $assetsSrc)
{
    New-Item -ItemType Directory -Path $assetsDst -Force | Out-Null
    Copy-Item -Path $assetsSrc\* -Destination $assetsDst -Recurse -Force
}

# 注册/卸载脚本
Copy-Item -Path (Join-Path $PSScriptRoot 'register.ps1') -Destination $outDir -Force
Copy-Item -Path (Join-Path $PSScriptRoot 'unregister.ps1') -Destination $outDir -Force
Copy-Item -Path (Join-Path $PSScriptRoot 'launch.cmd') -Destination $outDir -Force

# 尝试复制 VC++ 运行库（若可用）
function TryCopyVCRuntime {
    param(
        [string]$Platform
    )
    $possible = @()
    if ($env:VCToolsRedistDir)
    {
        $possible += (Join-Path -Path $env:VCToolsRedistDir -ChildPath (Join-Path $Platform 'Microsoft.VC143.CRT'))
        $possible += (Join-Path -Path $env:VCToolsRedistDir -ChildPath (Join-Path $Platform 'Microsoft.VC142.CRT'))
    }
    if ($env:VCINSTALLDIR)
    {
        $possible += (Join-Path -Path $env:VCINSTALLDIR -ChildPath 'Redist\MSVC')
    }

    $vsEditions = @('Community', 'Professional', 'Enterprise', 'BuildTools')
    $vsVersions = @('2022', '2019')
    $programFilesX86 = ${env:ProgramFiles(x86)}

    foreach ($version in $vsVersions)
    {
        foreach ($edition in $vsEditions)
        {
            if ($env:ProgramFiles)
            {
                $possible += (Join-Path -Path $env:ProgramFiles -ChildPath ("Microsoft Visual Studio\\{0}\\{1}\\VC\\Redist\\MSVC" -f $version, $edition))
            }
            if ($programFilesX86)
            {
                $possible += (Join-Path -Path $programFilesX86 -ChildPath ("Microsoft Visual Studio\\{0}\\{1}\\VC\\Redist\\MSVC" -f $version, $edition))
            }
        }
    }

    foreach ($p in $possible)
    {
        if (Test-Path $p)
        {
            $crtDir = $p
            if ((Get-Item $p).PSIsContainer -and (Split-Path $p -Leaf) -eq 'MSVC')
            {
                $crtDir = Get-ChildItem -Path $p -Directory |
                    Sort-Object -Property Name -Descending |
                    ForEach-Object { Join-Path -Path $_.FullName -ChildPath (Join-Path $Platform 'Microsoft.VC143.CRT') } |
                    Where-Object { Test-Path $_ } |
                    Select-Object -First 1
            }

            if ($crtDir -and (Test-Path $crtDir))
            {
                Get-ChildItem -Path $crtDir -Filter '*.dll' -File |
                    ForEach-Object { Copy-Item -Path $_.FullName -Destination $outDir -Force }
                return $true
            }
        }
    }

    Write-Warning '未能定位 VC++ 运行库目录。若目标机器没有安装 VC++ 运行库，可能会导致无法启动。'
    return $false
}

TryCopyVCRuntime -Platform $Platform | Out-Null

if ($IncludeVCLibs)
{
    $arch = if ($Platform -eq 'ARM64') { 'arm64' } else { 'x64' }
    $vclibsDir = Join-Path $outDir 'VCLibs'
    $vclibsPath = Join-Path -Path $vclibsDir -ChildPath ('Microsoft.VCLibs.{0}.14.00.Desktop.appx' -f $arch)
    if (!(Test-Path $vclibsPath))
    {
        New-Item -ItemType Directory -Path $vclibsDir -Force | Out-Null
        try
        {
            Invoke-WebRequest -Uri ('https://aka.ms/Microsoft.VCLibs.{0}.14.00.Desktop.appx' -f $arch) -OutFile $vclibsPath
        }
        catch
        {
            Write-Warning '未能下载 Microsoft.VCLibs 依赖。若目标机器缺少该依赖，右键菜单包可能无法注册。'
        }
    }
}

Write-Host ('已打包到：{0}' -f $outDir)
