# Telemetry（共享）

该目录包含 PowerToys 的 Telemetry 采集支持文件，用于问题定位与性能分析。
File Locksmith 一般不需要改动此模块，保留以保持上游结构一致。

## 快速使用
- 启动采集：`wpr.exe -start "PowerToys.wprp"`
- 停止采集：`wpr.exe -stop "Trace.etl"`
- 用 WPA 打开 `Trace.etl` 查看事件

## 原始说明
- 上游说明保留在 `readme.Original.md`（请勿修改）
