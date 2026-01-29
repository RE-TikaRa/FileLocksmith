# File Locksmith（单工具版）

File Locksmith 是用于定位并解除文件/文件夹占用的工具。
本项目从 PowerToys 0.97.1 的 File Locksmith 拆分而来，保留右键菜单体验，并新增管理主界面。
仅在 Windows 11 24H2 26100.7623 环境下完成验证。

## 界面预览

**首页**
![首页](https://s2.loli.net/2026/01/29/OuPSVzb8dCiMTa4.png)

**设置页**
![设置页](https://s2.loli.net/2026/01/29/2HFNnK9el8x6wI5.png)

**关于页**
![关于页](https://s2.loli.net/2026/01/29/NzBGtx9fDlRy1JY.png)

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
- `Logs\log.log`（原生日志）
- `Logs\Log_YYYY-MM-DD.log`（托管日志）
> 不再使用版本子目录，历史的 `Logs\<版本>` 可删除。

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

## 构建与打包

项目使用 WinUI 3 + Windows App SDK（.NET 9 目标）。

**输出目录**
- UI：`x64/Release/WinUI3Apps`
- CLI：`x64/Release/FileLocksmithCLI.exe`
- 便携包：`artifacts/FileLocksmithPortable/x64/Release`

**快捷脚本**
- 仅构建 UI：`build_project.bat`
- 构建 + 便携版打包：`build_and_pack.bat`

**清理缓存（推荐在重新构建前执行）**
- 删除 `x64/`
- 删除 `artifacts/FileLocksmithPortable/`
- 删除 `src/**/bin` 与 `src/**/obj`
- 删除旧版日志子目录 `Logs\<版本>`（如果仍存在）

**构建 UI（Release x64）**
```
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" src/modules/FileLocksmith/FileLocksmithUI/FileLocksmithUI.csproj /restore /p:Configuration=Release /p:Platform=x64 /p:RunAnalyzers=false /p:RunCodeAnalysis=false /p:EnableNETAnalyzers=false /p:EnforceCodeStyleInBuild=false /p:TreatWarningsAsErrors=false
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

**便携版构建/打包常见问题**
- **输出目录为空**：`pack.ps1` 依赖 `x64/Release/WinUI3Apps`。请先构建 UI，再打包。
- **便携版点击关于/设置闪退（MUI 资源缺失）**：若输出目录缺少 `FileLocksmithXAML`、`Assets` 或语言目录（如 `zh-CN`），WinUI 资源加载会崩溃。`pack.ps1` 已改为复制 `WinUI3Apps` 下所有子目录，若仍异常请重新构建并打包。

## 其他文档

- `tools/FileLocksmithPortable/README.md`：便携版打包与使用说明
- `src/common/Telemetry/README.md`：Telemetry 采集说明（排查/性能）
- `src/common/CalculatorEngineCommon/README.md`：exprtk 封装说明（共享库）
- `deps/spdlog/README.md`：第三方日志依赖说明
- 同目录下 `*.Original.md` 为上游/历史说明备份（即PowerToys官方代码说明）

## 依赖说明

- Windows App SDK 运行时可随包带上
- VC++ 运行库可随包带上（pack 脚本已尝试自动复制）

## 作者与信息

- 创建日期：2026-01-29
- 制作者：亓翎_Re-TikaRa
- 网站：https://re-tikara.fun

## 许可

本项目遵循 PowerToys 仓库的原始许可，即 MIT 许可，详见仓库根目录 LICENSE。

## 免责声明

本项目为开源研究与学习用途的软件分支，按“现状”提供，不对适用性、稳定性或特定用途做任何保证。  
使用本项目进行文件占用解除、进程终止、右键菜单注册/卸载等操作可能影响系统或数据，请自行确认风险并对操作结果负责。
