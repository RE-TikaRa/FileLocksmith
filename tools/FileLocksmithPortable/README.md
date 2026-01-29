# File Locksmith 便携版

本目录提供便携版打包脚本与使用说明。

## 脚本
- `pack.ps1`：将 UI + 右键菜单 + CLI + 依赖打包到便携目录

## 产物目录
`artifacts/FileLocksmithPortable/<平台>/<配置>/`

## 打包命令（示例）
```
powershell -ExecutionPolicy Bypass -File tools\FileLocksmithPortable\pack.ps1 -Platform x64 -Configuration Release
```

## 注意事项
- 需要先构建 UI，确保 `x64/Release/WinUI3Apps` 有产物。
- 便携目录内提供 `register.ps1` / `unregister.ps1` 进行注册与卸载。

## 原始说明
- 旧版说明已保留在 `README.Original.md`。