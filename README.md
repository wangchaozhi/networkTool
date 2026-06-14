# NetFloat

NetFloat 是一个基于 WPF / .NET 8 的 Windows 桌面悬浮网速工具，用于实时显示网络上传和下载速度。

## 功能

- 实时显示上传/下载速度，支持 KB/s、MB/s、GB/s。
- 自动选择合适的活动网络接口。
- 系统托盘菜单控制显示、隐藏、退出、主题、字体、缩放和圆角。
- 支持开机自启。
- 支持文件拖拽，复制文件路径到剪贴板并播放动画。
- 配置保存至 `%AppData%\NetFloat\config.json`。
- 支持 GitHub Actions 手动发布 6 种 Windows 资产。

## 技术栈

| 依赖 | 版本 |
| --- | --- |
| .NET / WPF | 8.0 |
| HandyControl | 3.5.1 |
| Hardcodet.Wpf.TaskbarNotification | 1.0.5 |
| Newtonsoft.Json | 13.0.3 |

## 运行要求

- Windows 10 / 11
- .NET 8 Desktop Runtime，除非使用 self-contained 发布包

## 构建

```bash
dotnet build NetFloat/NetFloat.csproj
```

发布示例：

```bash
dotnet publish NetFloat/NetFloat.csproj -c Release -r win-x86 --self-contained false
```


## 说明

当前项目使用 WPF，因此运行平台为 Windows。若要跨平台，需要迁移到 Avalonia UI 等跨平台桌面框架。
