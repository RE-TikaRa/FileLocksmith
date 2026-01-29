# Repository Guidelines

## 项目结构与模块组织
- `src/modules/FileLocksmith/` 为核心功能模块：
  - `FileLocksmithUI/` 管理主界面（WinUI 3，.NET 9 目标）
  - `FileLocksmithContextMenu/` Win11 右键菜单处理
  - `FileLocksmithExt/` 经典右键菜单处理
  - `FileLocksmithLib/` 原生核心逻辑与 IPC
  - `FileLocksmithLibInterop/` 原生互操作层
  - `FileLocksmithCLI/` 命令行入口与单元测试
- `src/common/` 共享库与通用工具
- `tools/FileLocksmithPortable/` 便携版打包脚本
- `deps/` 第三方 C++ 依赖（spdlog、expected-lite）

## 输出目录
- UI 输出：`x64/Release/WinUI3Apps/`
- CLI 输出：`x64/Release/FileLocksmithCLI.exe`
- 便携包输出（自带依赖）：`artifacts/FileLocksmith Portable version/x64/Release/`
- 便携包输出（系统依赖）：`artifacts/FileLocksmith System dependency versions/x64/Release/`

## 构建、测试与开发命令
- 仅构建 UI：`build_project.bat`
- 构建 + 便携版打包：`build_and_pack.bat`（同时输出 Portable/System 两种）
- 生成便携包：`tools/FileLocksmithPortable/pack.ps1`
- 需要完整构建时，优先使用 VS 2022 + MSBuild 调用各 `.csproj/.vcxproj`

## 清理缓存（重建前建议）
- 删除 `x64/`
- 删除 `artifacts/FileLocksmith Portable version/`
- 删除 `artifacts/FileLocksmith System dependency versions/`
- 删除 `src/**/bin` 与 `src/**/obj`
- 删除旧版日志子目录 `Logs\\<版本>`（若存在）

## 便携包注意事项
- `pack.ps1` 依赖 `x64/Release/WinUI3Apps` 产物
- System 模式使用 `x64/Release/WinUI3Apps.System` 产物
- 便携包需包含 `WinUI3Apps` 下的全部资源目录（含语言包），否则页面切换可能崩溃
- `register.ps1` / `unregister.ps1` 可能需要管理员权限

## 编码风格与命名约定
- C++ 头/源文件成对，类型 `PascalCase`，局部变量 `camelCase`
- C# 类型与成员 `PascalCase`，命名空间与目录保持一致
- C# 规则由 `src/codeAnalysis/StyleCop.json` 控制

## 测试指南
- 单元测试位于 `src/modules/FileLocksmith/FileLocksmithCLI/tests`
- 使用 Microsoft C++ Unit Test Framework（`CppUnitTest.h`）
- 通过 Visual Studio Test Explorer 运行 `FileLocksmithCLIUnitTests.vcxproj`（x64）

## 文档维护规则
- 维护范围：根目录 `README.md`、`tools/FileLocksmithPortable/README.md`、`AGENTS.md`
- 不修改 `*.Original.md` 与微软原始说明文档，除非明确要求
