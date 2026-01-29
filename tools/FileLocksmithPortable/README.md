# File Locksmith 便携版

本目录提供便携版打包脚本与使用说明。

## 目录内容

- `pack.ps1`：将 UI + 右键菜单 + CLI + 依赖打包到便携目录
- `register.ps1` / `unregister.ps1`：注册与卸载右键菜单
- `launch.cmd`：启动主界面

## 产物目录

`artifacts/FileLocksmithPortable/<平台>/<配置>/`

## 打包流程

1. 构建 UI，确保 `x64/Release/WinUI3Apps` 有产物
2. （可选）构建 CLI 与原生组件，确保对应输出存在
3. 执行打包脚本

```
powershell -ExecutionPolicy Bypass -File tools\\FileLocksmithPortable\\pack.ps1 -Platform x64 -Configuration Release
```

## 产物内容（关键项）

- `PowerToys.FileLocksmithUI.exe`（主界面）
- `FileLocksmithCLI.exe`（命令行）
- `FileLocksmithContextMenu.dll` / `FileLocksmithExt.dll`（右键菜单）
- `FileLocksmithXAML`、`Assets`、语言包目录（WinUI 资源）
- Windows App SDK Runtime 与 VC++ 运行库（脚本会尝试复制）

## 注意事项

- 便携包必须包含 `WinUI3Apps` 下的全部资源目录（含语言包），否则可能在打开页面时崩溃。
- 右键菜单注册/卸载可能需要管理员权限，请按需以管理员运行脚本。

## 原始说明

- 旧版说明已保留在 `README.Original.md`，不建议修改。
