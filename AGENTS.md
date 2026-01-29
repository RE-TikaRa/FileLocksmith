# Repository Guidelines

## 项目结构与模块组织
- `src/modules/FileLocksmith/` 为核心功能模块：
  - `FileLocksmithUI/` 管理主界面（WinUI 3，.NET 9 目标）。
  - `FileLocksmithContextMenu/` Win11 右键菜单处理。
  - `FileLocksmithExt/` 经典右键菜单处理。
  - `FileLocksmithLib/` 原生核心逻辑与 IPC。
  - `FileLocksmithLibInterop/` 原生互操作层。
  - `FileLocksmithCLI/` 命令行入口与单元测试。
- `src/common/` 为共享库与通用工具。
- `tools/FileLocksmithPortable/` 为便携版打包脚本。

## 构建、测试与开发命令
- 仅构建 UI：`build_project.bat`
- 构建 + 便携版打包：`build_and_pack.bat`
- 手动构建 UI（Release x64）：
  - `MSBuild.exe src/modules/FileLocksmith/FileLocksmithUI/FileLocksmithUI.csproj /restore /p:Configuration=Release /p:Platform=x64`
- 构建 CLI 与原生项目：使用 Visual Studio 2022 打开对应 `.vcxproj`，选择 x64 配置构建。
- 生成便携包：
  - `tools/FileLocksmithPortable/pack.ps1`（PowerShell 脚本，可能需要管理员权限）。

## 输出目录
- UI 默认输出：`x64/Release/WinUI3Apps/`
- CLI 输出：`x64/Release/FileLocksmithCLI.exe`
- 便携包输出：`artifacts/FileLocksmithPortable/x64/Release/`

## 编码风格与命名约定
- 延续现有风格：C++ 头/源文件成对，类型 `PascalCase`，局部变量 `camelCase`。
- C# 类型与成员用 `PascalCase`，命名空间与目录保持一致。
- C# 规则由 `src/codeAnalysis/StyleCop.json` 控制，修改 UI 时保持 StyleCop 通过。

## 测试指南
- 单元测试位于 `src/modules/FileLocksmith/FileLocksmithCLI/tests`。
- 使用 Microsoft C++ Unit Test Framework（`CppUnitTest.h`）。
- 通过 Visual Studio Test Explorer 运行 `FileLocksmithCLIUnitTests.vcxproj`（x64）。

## 提交与 PR 规范
- 现有 Git 记录仅有 “first commit”，暂无固定提交规范。
- 建议使用简洁祈使句提交标题（如 “Add CLI JSON output”）。
- PR 请包含变更说明、构建/测试结果；涉及界面变化时附截图。

## 安全与配置提示
- 项目在 Windows 11 24H2 上验证通过；右键菜单注册/卸载可能需要提权。
- 便携包需要包含 WinUI3Apps 的资源目录（含语言包），否则可能触发页面打开崩溃。
