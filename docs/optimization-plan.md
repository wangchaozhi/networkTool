# NetFloat 优化技术文档

## 项目现状

NetFloat 是一个基于 .NET 8 WPF 的 Windows 桌面悬浮工具。当前主要能力包括：

- 悬浮窗展示上传和下载速度。
- 托盘菜单控制显示隐藏、主题、字体、缩放、圆角、开机自启和更新检查。
- 配置保存到 `%AppData%\NetFloat\config.json`。
- 文件拖拽后复制路径并播放窗口动画。

## 已完成优化

- 修正网速监控初始化重复问题。
- 移除网速监控相关假异步方法。
- 增强配置文件容错和默认值回退。
- 清理 `MainWindow` 中明显无用字段和旧逻辑。
- 修复构建警告，保持 `dotnet build` 0 警告 0 错误。
- 统一应用对外名称为 `NetFloat`。

## 后续建议

- 抽象 `NetworkSpeedService`，通过事件或绑定向 ViewModel 暴露速度文本。
- 将上传和下载速度改为 ViewModel 属性绑定，减少 code-behind 直接操作控件。
- 更新托盘图标依赖，消除对旧 .NET Framework 包的兼容依赖。
- 为设置保存增加防抖，避免滑块快速变化时频繁写入配置文件。
- 若需要跨平台，迁移到 Avalonia UI 等跨平台桌面框架。
