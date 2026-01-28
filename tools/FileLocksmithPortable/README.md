# File Locksmith 便携版打包与使用

本目录提供将 File Locksmith 单独打包为“便携版”所需的脚本。
目标：把所有功能（UI + 右键菜单 + CLI）独立打包成一个可复制的文件夹，并附带注册/卸载脚本。

## 目录结构（打包产物）
打包脚本会生成：
`artifacts/FileLocksmithPortable/<平台>/<配置>/`

该目录包含：
- FileLocksmith UI（`PowerToys.FileLocksmithUI.exe`）
- Shell 扩展（`PowerToys.FileLocksmithExt.dll`）
- Win11 右键菜单包（`FileLocksmithContextMenuPackage.msix`）
- CLI（`FileLocksmithCLI.exe`）
- 依赖 DLL 与资源文件（含 WinAppSDK 本地运行时）
- `register.ps1` / `unregister.ps1`

## 打包
在仓库根目录运行：

```
# x64 Release 示例
powershell -ExecutionPolicy Bypass -File tools\FileLocksmithPortable\pack.ps1 -Platform x64 -Configuration Release
```

> 若已手动构建相关项目，脚本会直接打包现有产物。

## 使用（目标机器）
1. 将 `artifacts/FileLocksmithPortable/...` 目录完整复制到目标机器任意位置。
2. 运行 `register.ps1` 完成右键菜单注册与 Win11 菜单包注册。
3. 使用时：
   - 右键文件/文件夹选择“Unlock with File Locksmith”
   - 或直接运行 `PowerToys.FileLocksmithUI.exe`
   - CLI：`FileLocksmithCLI.exe`

## 卸载（目标机器）
运行 `unregister.ps1` 取消注册，然后删除文件夹即可。

## 依赖说明（保证“无缺失依赖”）
- UI 为 `SelfContained`，WinAppSDK 运行时会随打包产物一起复制。
- 原生模块依赖 VC++ 运行库，打包脚本会尝试从本机构建环境复制 VC++ 运行库到便携目录；若复制失败，会给出提示。
- 打包脚本默认下载并内置 `Microsoft.VCLibs.140.00`（位于 `VCLibs/`），注册时优先使用本地依赖；如未包含且允许联网，会自动下载。
