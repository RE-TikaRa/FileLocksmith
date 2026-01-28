# File Locksmith（单工具版）

File Locksmith 是一个用于定位并解除文件/文件夹占用的工具。本项目为从 PowerToys 中分离出的单工具版本，保留原有右键菜单体验，同时新增管理主界面。仅在 Windows 11 24H2 26100.7623 环境下完成验证。

## 功能概览

- 右键菜单：快速定位占用文件/文件夹的进程并处理
- 扫描界面：展示占用进程、文件列表，可结束进程
- 管理主界面：统一管理注册状态与显示方式
- 支持“仅扩展菜单显示”（Win11“显示更多选项”）
- 支持提升权限完成注册/卸载与系统进程查看

## 运行方式

- 直接运行：启动管理主界面
- 从右键菜单启动：进入扫描/解锁界面
- CLI：`FileLocksmithCLI.exe`

## 配置与数据位置（独立路径）

**根目录**
`%LocalAppData%\ALp_Studio\FileLocksmith`

**设置文件**
`file-locksmith-settings.json`
- `Enabled`：是否启用
- `showInExtendedContextMenu`：仅扩展菜单显示

**运行数据**
`last-run.log`：上次选择路径列表（UTF-16 + 换行，空行终止）

**日志**
`Logs\<版本>\log.log`

## 组策略（GPO）

管理员可通过策略强制启用/禁用：

`HKLM\Software\Policies\FileLocksmith`
- `Enabled`（DWORD）：`1` 启用，`0` 禁用  
若存在该键值，将覆盖本地设置。

## 注册表标记（状态）

用于记录右键菜单注册状态（脚本写入）：

`HKCU\Software\FileLocksmith`
- `ContextMenuRegistered`（DWORD）

## CLI 使用

```
FileLocksmithCLI.exe [选项] <路径1> [路径2] ...
选项:
  --kill      结束占用文件的进程
  --json      以 JSON 格式输出结果
  --wait      等待文件解锁
  --timeout   --wait 的超时（毫秒）
  --help      显示帮助
```

## 结构速览

- `src/modules/FileLocksmith/FileLocksmithUI`
  - 管理主界面与 WinUI 视图
- `src/modules/FileLocksmith/FileLocksmithContextMenu`
  - 右键菜单（Win11）
- `src/modules/FileLocksmith/FileLocksmithExt`
  - 右键菜单（经典）
- `src/modules/FileLocksmith/FileLocksmithLib`
  - 原生核心逻辑（句柄枚举/扫描）
- `src/modules/FileLocksmith/FileLocksmithLibInterop`
  - 原生互操作层（WinRT）
- `src/modules/FileLocksmith/FileLocksmithCLI`
  - 命令行与单元测试
- `tools/FileLocksmithPortable`
  - 便携版打包脚本

## 构建说明

项目使用 WinUI 3 + Windows App SDK（.NET 9 目标）。构建输出默认位于：

- `x64/Release/WinUI3Apps`

**构建 UI（Release x64）**
```
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" src/modules/FileLocksmith/FileLocksmithUI/FileLocksmithUI.csproj /p:Configuration=Release /p:Platform=x64 /p:DisableSpectreMitigation=true /p:TreatWarningsAsErrors=false /p:RunAnalyzersDuringBuild=false /p:EnableNETAnalyzers=false
```

**构建 CLI（Release x64）**
```
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" src/modules/FileLocksmith/FileLocksmithCLI/FileLocksmithCLI.vcxproj /p:Configuration=Release /p:Platform=x64
```

**构建原生组件（Release x64）**
```
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" src/modules/FileLocksmith/FileLocksmithLib/FileLocksmithLib.vcxproj /p:Configuration=Release /p:Platform=x64
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" src/modules/FileLocksmith/FileLocksmithLibInterop/FileLocksmithLibInterop.vcxproj /p:Configuration=Release /p:Platform=x64
```

**构建右键菜单组件（Release x64）**
```
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" src/modules/FileLocksmith/FileLocksmithContextMenu/FileLocksmithContextMenu.vcxproj /p:Configuration=Release /p:Platform=x64
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" src/modules/FileLocksmith/FileLocksmithExt/FileLocksmithExt.vcxproj /p:Configuration=Release /p:Platform=x64
```

**CLI 与原生项目（可选）**
- 使用 Visual Studio 2022 打开对应 `.vcxproj`，选择 x64 构建。

## 便携包

```
powershell -ExecutionPolicy Bypass -File tools\FileLocksmithPortable\pack.ps1 -Platform x64 -Configuration Release
```

## 依赖说明

- Windows App SDK 运行时可随包带上
- VC++ 运行库可随包带上（pack 脚本已尝试自动复制）

## 作者与信息

- 创建日期：2026-01-29
- 制作者：亓翎_Re-TikaRa
- 网站：https://re-tikara.fun

## 许可

本项目遵循 PowerToys 仓库的原始许可，即 MIT 许可，详见仓库根目录 LICENSE。
