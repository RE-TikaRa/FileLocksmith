param(
    [switch]$AllowDownload = $true
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$extDll = Join-Path $root 'PowerToys.FileLocksmithExt.dll'
$msix = Join-Path $root 'FileLocksmithContextMenuPackage.msix'

if (!(Test-Path $extDll))
{
    throw "找不到 $extDll。请确认已正确打包。"
}

$clsid = '{84D68575-E186-46AD-B0CB-BAEB45EE29C0}'
$clsidKey = "HKCU:\Software\Classes\CLSID\$clsid"
$inprocKey = "$clsidKey\InprocServer32"

New-Item -Path $clsidKey -Force | Out-Null
New-Item -Path $inprocKey -Force | Out-Null

Set-ItemProperty -Path $clsidKey -Name '(default)' -Value 'File Locksmith Shell Extension'
Set-ItemProperty -Path $clsidKey -Name 'ContextMenuOptIn' -Value ''
Set-ItemProperty -Path $inprocKey -Name '(default)' -Value $extDll
Set-ItemProperty -Path $inprocKey -Name 'ThreadingModel' -Value 'Apartment'

$handlerKeys = @(
    'HKCU:\Software\Classes\AllFileSystemObjects\ShellEx\ContextMenuHandlers\FileLocksmithExt',
    'HKCU:\Software\Classes\Drive\ShellEx\ContextMenuHandlers\FileLocksmithExt'
)

foreach ($key in $handlerKeys)
{
    New-Item -Path $key -Force | Out-Null
    Set-ItemProperty -Path $key -Name '(default)' -Value $clsid
}

$sentinelKey = 'HKCU:\Software\Microsoft\PowerToys\FileLocksmith'
New-Item -Path $sentinelKey -Force | Out-Null
New-ItemProperty -Path $sentinelKey -Name 'ContextMenuRegistered' -PropertyType DWord -Value 1 -Force | Out-Null

# Win11 右键菜单包（如存在）
$osVersion = [System.Environment]::OSVersion.Version
$win11OrGreater = ($osVersion.Major -gt 10) -or ($osVersion.Major -eq 10 -and $osVersion.Build -ge 22000)

if ($win11OrGreater -and (Test-Path $msix))
{
    $pkg = Get-AppxPackage -Name 'FileLocksmithContextMenu' -ErrorAction SilentlyContinue
    if (!$pkg)
    {
        try
        {
            Add-AppxPackage -Path $msix
        }
        catch
        {
            $vclibs = Get-AppxPackage -Name 'Microsoft.VCLibs.140.00.Desktop' -ErrorAction SilentlyContinue
            if ($vclibs)
            {
                throw
            }

            $arch = if ($env:PROCESSOR_ARCHITECTURE -eq 'ARM64') { 'arm64' } else { 'x64' }
            $localVCLibs = Join-Path $root "VCLibs\Microsoft.VCLibs.$arch.14.00.Desktop.appx"

            if (Test-Path $localVCLibs)
            {
                Add-AppxPackage -Path $localVCLibs
            }
            elseif ($AllowDownload)
            {
                $tmp = Join-Path $env:TEMP "Microsoft.VCLibs.$arch.14.00.Desktop.appx"
                Invoke-WebRequest -Uri "https://aka.ms/Microsoft.VCLibs.$arch.14.00.Desktop.appx" -OutFile $tmp
                Add-AppxPackage -Path $tmp
                Remove-Item -Path $tmp -Force
            }
            else
            {
                throw "未安装 Microsoft.VCLibs.140.00.Desktop，且未允许自动下载。"
            }

            Add-AppxPackage -Path $msix
        }
    }
}

Write-Host 'File Locksmith 已注册。右键菜单可能需要重启资源管理器后生效。'
