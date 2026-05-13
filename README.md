# 网速浮窗

一个基于 WPF / .NET 8 的 Windows 桌面悬浮工具，实时显示网络上传/下载速度，支持丰富的外观自定义。

## 功能

- **实时网速**：自动识别最佳网络接口，每秒刷新上传/下载速度（KB/s · MB/s · GB/s）
- **系统托盘**：右键菜单控制所有功能，双击托盘图标显示/隐藏浮窗
- **主题**：默认白色 / 透明 / 小麦色
- **字体**：Arial · Times New Roman · 楷体 · 宋体 · 黑体
- **缩放**：0.6× · 0.8× · 1× · 2× · 3×
- **圆角**：滑块实时调节边框圆角
- **图标**：默认 / 棕色风格切换
- **文件拖拽**：将文件拖入浮窗，自动复制路径到剪贴板，触发文件飞入动画
- **开机自启**：一键设置/取消 Windows 开机启动
- **在线更新**：检查并拉取新版本
- **木鱼窗口**：太极图样式的木鱼小窗口
- **配置持久化**：设置保存至 `%AppData%\MyApp\config.json`，下次启动自动还原

## 截图

> 浮窗默认贴靠屏幕右上角，可自由拖动

## 技术栈

| 依赖 | 版本 |
|------|------|
| .NET / WPF | 8.0 (win-x86) |
| HandyControl | 3.5.1 |
| Hardcodet.Wpf.TaskbarNotification | 1.0.5 |
| Newtonsoft.Json | 13.0.3 |

## 运行要求

- Windows 10 / 11
- [.NET 8 Desktop Runtime (x86)](https://dotnet.microsoft.com/download/dotnet/8.0)

## 构建

```bash
dotnet build WpfApp2/WpfApp2.csproj
```

发布单文件：

```bash
dotnet publish WpfApp2/WpfApp2.csproj -c Release -r win-x86 --self-contained false
```

## 版本历史

| Tag | 说明 |
|-----|------|
| v0.3.1 | 代码优化：消除假 async、修复 Timer 双采样、统一配置容错、新增木鱼窗口 |
| v0.3 | 图标切换、文件拖拽复制路径、FaceWindow 动画、在线更新、用户配置持久化 |
| v0.2 | 太极窗口、圆角滑块、缩放、字体、主题切换 |
| v0.1.0 | 初始版本：网速悬浮窗 |

## 许可

本项目仅供个人学习使用。
