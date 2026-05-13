# NetFloat 优化技术文档

## 项目现状

NetFloat 是一个基于 .NET 8 WPF 的 Windows 桌面悬浮工具。当前主要能力包括：

- 悬浮窗展示上传和下载速度。
- 托盘菜单控制显示隐藏、主题、字体、缩放、圆角、开机自启和更新检查。
- 配置保存到 `%AppData%\NetFloat\config.json`。
- 文件拖拽后复制路径并播放窗口动画。

## 优化目标

第一阶段以低风险稳定性优化为主，不改变现有交互和视觉表现：

- 减少重复计时器启动，避免网速监控逻辑出现竞态或多余初始化。
- 移除没有实际异步工作的 `async` 声明，让方法语义更准确。
- 增强配置文件容错，避免配置损坏导致程序无法启动。
- 清理明显无用的字段和旧逻辑入口，降低维护成本。
- 保持 `dotnet build` 通过。

## 发现的问题

### 1. 网速监控初始化重复

`MainWindow_Loaded` 调用 `InitializeNetworkInterfaceAsync()` 后又调用 `StartMonitoringAsync()`，而 `InitializeNetworkInterfaceAsync()` 内部也会启动监控。这会造成初始化阶段重复停止和创建 `DispatcherTimer`。

优化方式：

- 将网卡初始化和监控启动拆清楚。
- `Loaded` 中只初始化一次，并由初始化成功后启动监控。
- 网络变化事件触发时重新初始化，并重启监控。

### 2. 假异步方法

`InitializeNetworkInterfaceAsync()` 和 `StartMonitoringAsync()` 当前没有 `await`，实际是同步逻辑。继续使用 `async Task` 会让调用方误以为存在异步调度。

优化方式：

- 改为同步方法 `InitializeNetworkInterface()` 和 `StartMonitoring()`。
- 对事件处理中的调用使用普通同步调用。

### 3. 配置文件容错不足

`ConfigurationManager` 在配置文件存在时直接 `JObject.Parse`。如果 JSON 文件为空、损坏或缺少 `Settings` 节点，启动或读取配置时可能异常。

优化方式：

- 抽出默认配置创建逻辑。
- 读取失败时备份损坏配置，并重建默认配置。
- 获取配置时如果键不存在，返回默认配置中的值。
- 设置配置前确保 `Settings` 节点存在。

### 4. 代码职责偏重

`MainWindow.xaml.cs` 同时包含网卡选择、测速、窗口拖动、拖拽文件动画和 UI 文本更新。后续建议抽出 `NetworkSpeedService`，但第一阶段不做大重构。

### 5. 第三方包兼容警告

构建时出现 `NU1701`，提示 `Hardcodet.NotifyIcon.Wpf 1.0.5` 以 .NET Framework 兼容方式还原。当前不阻塞构建，但后续建议替换为支持现代 .NET 的托盘库，或锁定验证过的运行环境。

## 第一阶段执行清单

- [x] 修正网速监控初始化重复（`Loaded` 与 `InitializeNetworkInterface` 内部双重启动 timer）。
- [x] 移除网速监控相关假异步（`InitializeNetworkInterfaceAsync` / `StartMonitoringAsync` 无 await）。
- [x] 增强配置文件容错和默认值回退（`ConfigurationManager` 已实现）。
- [x] 清理 `MainWindow` 中大量注释代码、未使用字段和无用方法。
- [x] 修正 `Timer_Tick` 中 `GetIPv4Statistics()` 被调用两次（两次读取间数据可能变化）。
- [x] `DispatcherTimer` 已在 UI 线程触发，`Timer_Tick` 内 `Dispatcher.Invoke` 为冗余，移除。
- [x] 注册表 `RegistryKey` 未用 `using` 释放（`App.xaml.cs` SetApplicationToRunAtStartup / RemoveApplicationFromStartup）。
- [x] `UpdateScale` 内部冗余赋值 `CurrentScale = scale`，移除（setter 已调用 `UpdateScale`，会多触发一次写配置）。
- [x] `_configManager` 降级为 `private static`，`App` 自持引用，不再通过 `MainViewModel._configManager` 跨类访问。
- [x] 统一 `PropertyChanged` 通知方法，去掉 `NotifyPropertyChanged`，统一使用 `OnPropertyChanged`。
- [x] `HttpClient` 改为静态实例，避免 `CheckForUpdates` 每次 new 新实例。
- [x] `FormatSpeed` 魔法数字提取为常量。
- [x] 运行 `dotnet build` 验证，0 个错误。

## 后续建议

- 抽象 `NetworkSpeedService`，通过事件或绑定向 ViewModel 暴露速度文本。
- 将上传和下载速度改为 ViewModel 属性绑定，减少 code-behind 直接操作控件。
- 保持应用名统一为 `NetFloat`。
- 更新托盘图标依赖，消除 `NU1701` 兼容警告。
- 为设置保存增加防抖，避免滑块快速变化时频繁写入配置文件。
