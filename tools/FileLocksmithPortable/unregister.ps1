$ErrorActionPreference = 'Stop'

$clsid = '{84D68575-E186-46AD-B0CB-BAEB45EE29C0}'

$keysToRemove = @(
    "HKCU:\Software\Classes\AllFileSystemObjects\ShellEx\ContextMenuHandlers\FileLocksmithExt",
    "HKCU:\Software\Classes\Drive\ShellEx\ContextMenuHandlers\FileLocksmithExt",
    "HKCU:\Software\Classes\CLSID\$clsid"
)

foreach ($key in $keysToRemove)
{
    if (Test-Path $key)
    {
        Remove-Item -Path $key -Recurse -Force
    }
}

$sentinelKey = 'HKCU:\Software\FileLocksmith'
if (Test-Path $sentinelKey)
{
    Remove-ItemProperty -Path $sentinelKey -Name 'ContextMenuRegistered' -ErrorAction SilentlyContinue
}

$pkg = Get-AppxPackage -Name 'FileLocksmithContextMenu' -ErrorAction SilentlyContinue
if ($pkg)
{
    Remove-AppxPackage -Package $pkg.PackageFullName
}

Write-Host 'File Locksmith 已取消注册。'
