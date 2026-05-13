using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using WpfApp2.Windows;


namespace WpfApp2;

public partial class MainWindow 
{
    private long lastBytesSent = 0;
    private long lastBytesReceived = 0;
    private DispatcherTimer timer;

    private TaskbarIcon notifyIcon;
    // public ICommand OpenCommand { get; private set; }
    // public ICommand ExitCommand { get; private set; }
    private TaiChiWindow taiChiWindow;
    public MainWindow()
    {
        InitializeComponent();
        // taiChiWindow = new TaiChiWindow();
        // taiChiWindow.Show();
        // GlassWindowManager.EnableGlassEffect(this);
        // 注册窗体关闭事件以清理资源
        // this.Closed += MainWindow_Closed;
        // this.SourceInitialized += MainWindow_SourceInitialized;
        // 设置窗口启动位置
        this.Loaded += MainWindow_Loaded;
        this.Closed += MainWindow_Closed;
        // InitializeNetworkInterface();
        // StartMonitoring();
    }

    private void MainWindow_Closed(object sender, EventArgs e)
    {
        // 在窗体关闭时注销事件处理程序
        NetworkChange.NetworkAddressChanged -= NetworkAddressChanged;
        NetworkChange.NetworkAvailabilityChanged -= NetworkAvailabilityChanged;
        this.Closed -= MainWindow_Closed;
    }
    
    
    // protected override void OnLocationChanged(EventArgs e)
    // {
    //     base.OnLocationChanged(e);
    //     if (taiChiWindow != null)
    //     {
    //         taiChiWindow.Left = this.Left;
    //         taiChiWindow.Top = this.Top;
    //     }
    // }
    //
    
    // protected override void OnLocationChanged(EventArgs e)
    // {
    //     base.OnLocationChanged(e);
    //     if (taiChiWindow != null)
    //     {
    //         // 确保获取正确的宽度，特别是如果 TaiChiWindow 还未完全加载
    //         double taiChiWidth = taiChiWindow.Width*0.2;
    //         // // 确保获取正确的宽度，特别是如果 TaiChiWindow 还未完全加载
    //         double taiChiHeight = taiChiWindow.Height*0.2;
    //         // if (taiChiHeight == 0) {
    //         //     taiChiHeight = taiChiWindow.Height;
    //         // }
    //
    //         // 设置 TaiChiWindow 的左边缘，使其中心对齐 MainWindow 的左边缘
    //         taiChiWindow.Left = this.Left - taiChiWidth / 2;
    //         taiChiWindow.Top = this.Top;
    //     }
    // }


    private void MainWindow_SourceInitialized(object sender, EventArgs e)
    {
        var hWnd = new WindowInteropHelper(this).Handle;
        EnableGlassEffect(hWnd);
    }

    private void EnableGlassEffect(IntPtr hWnd)
    {
        var margins = new GlassEffect.MARGINS
            { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
        GlassEffect.DwmExtendFrameIntoClientArea(hWnd, ref margins);

        var bb = new GlassEffect.DWM_BLURBEHIND
        {
            dwFlags = GlassEffect.DWM_BB_ENABLE,
            fEnable = true,
            hRgnBlur = IntPtr.Zero,
            fTransitionOnMaximized = true
        };
        GlassEffect.DwmEnableBlurBehindWindow(hWnd, ref bb);
    }


    // private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    // {
    //     // 获取屏幕工作区域的宽度和高度
    //     double screenWidth = SystemParameters.WorkArea.Width;
    //     double screenHeight = SystemParameters.WorkArea.Height;
    //
    //     // 设置窗口在屏幕右上角
    //     this.Left = screenWidth/1.15;
    //     this.Top = screenHeight/22;
    // }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 获取屏幕工作区域的宽度和高度
        double screenWidth = SystemParameters.WorkArea.Width;
        double screenHeight = SystemParameters.WorkArea.Height;

        // 设置窗口在屏幕右上角
        this.Left = screenWidth / 1.15;
        this.Top = screenHeight / 22;

        RegisterNetworkChangeEvents(); // 确保监听器被注册

        // 异步初始化网络接口和启动监控
        await InitializeNetworkInterfaceAsync();
        await StartMonitoringAsync();
    }
    
    


    private void RegisterNetworkChangeEvents()
    {
        NetworkChange.NetworkAddressChanged += NetworkAddressChanged;
        NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChanged;
    }

    private void NetworkAddressChanged(object sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            InitializeNetworkInterfaceAsync(); // 重新初始化网络接口
        });
    }

    private void NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (e.IsAvailable)
            {
                InitializeNetworkInterfaceAsync(); // 网络变为可用时重新初始化
            }
        });
    }

    // private async Task InitializeNetworkInterfaceAsync()
    // {
    //     NetworkInterface bestInterface = null;
    //     long maxData = 0;
    //
    //     foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
    //     {
    //         if (ni.OperationalStatus == OperationalStatus.Up &&
    //             ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
    //         {
    //             IPv4InterfaceStatistics stats = ni.GetIPv4Statistics();
    //             long totalData = stats.BytesSent + stats.BytesReceived;
    //             if (totalData > maxData)
    //             {
    //                 bestInterface = ni;
    //                 maxData = totalData;
    //             }
    //         }
    //     }
    //
    //     if (bestInterface != null)
    //     {
    //         Console.WriteLine($"Selected interface: {bestInterface.Description}");
    //         // 可以在这里进行进一步的操作，比如配置该接口或更新UI
    //     }
    //     else
    //     {
    //         Console.WriteLine("No suitable network interface found.");
    //         // 可以选择是否要在这里注册网络变化监听，如果未注册
    //         RegisterNetworkChangeEvents();
    //     }
    // }
    //

    private async Task InitializeNetworkInterfaceAsync()
    {
        NetworkInterface bestInterface = null;
        long maxData = 0;
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            // 排除虚拟接口和非活跃接口
            if (ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel && // 排除隧道接口
                ni.NetworkInterfaceType != NetworkInterfaceType.Ppp && // 排除点对点协议接口
                !ni.Description.ToLowerInvariant().Contains("virtual") && // 排除描述中包含"virtual"的接口
                !ni.Description.ToLowerInvariant().Contains("vpn") && // 排除描述中包含"vpn"的接口
                !ni.Description.ToLowerInvariant().Contains("pseudo") && // 排除假接口
                !ni.Description.ToLowerInvariant().Contains("vmware")) // 排除描述中包含"vmware"的接口
            {
                IPv4InterfaceStatistics stats = ni.GetIPv4Statistics();
                long totalData = stats.BytesSent + stats.BytesReceived;
                if (totalData > maxData)
                {
                    bestInterface = ni;
                    maxData = totalData;
                }
            }
        }

        if (bestInterface != null)
        {
            Console.WriteLine($"Selected interface: {bestInterface.Description}");
            currentInterface = bestInterface; // 更新当前接口
            var initialStats = currentInterface.GetIPv4Statistics();
            lastBytesSent = initialStats.BytesSent;
            lastBytesReceived = initialStats.BytesReceived;
            // 重新启动监控，确保监控当前选择的接口
            StartMonitoringAsync();
        }
        else
        {
            Console.WriteLine("No suitable network interface found.");
        }
    }


    private NetworkInterface currentInterface;


    private async Task StartMonitoringAsync()
    {
        if (timer != null)
        {
            timer.Stop(); // 停止当前的计时器
            timer.Tick -= Timer_Tick; // 取消订阅事件
        }
        
        if (currentInterface == null)
        {
            Console.WriteLine("No active network interface available for monitoring.");
            return;
        }

        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
        timer.Start();
    }


    private void Timer_Tick(object sender, EventArgs e)
    {
        long bytesSent = currentInterface.GetIPv4Statistics().BytesSent;
        long bytesReceived = currentInterface.GetIPv4Statistics().BytesReceived;

        long sentSpeed = bytesSent - lastBytesSent;
        long receivedSpeed = bytesReceived - lastBytesReceived;
        
        // 检查是否有接口切换或计数器回绕
        if (sentSpeed < 0 || receivedSpeed < 0)
        {
            // 可能是接口切换或计数器回绕，重新初始化速度统计
            sentSpeed = 0;
            receivedSpeed = 0;
        }

        lastBytesSent = bytesSent;
        lastBytesReceived = bytesReceived;

        // 此处应更新UI（回到UI线程）
        Dispatcher.Invoke(() =>
        {
            // 假设存在名为uploadSpeedText和downloadSpeedText的UI元素
            uploadSpeedText.Text = FormatSpeed(sentSpeed);
            downloadSpeedText.Text = FormatSpeed(receivedSpeed);
        });
    }

    private string FormatSpeed(long speed)
    {
        double speedInKBps = speed / 1024.0;  // 将速度转换为KB/s
        if (speedInKBps >= 1048576)  // 如果速度超过1048576 KB/s，即超过1 GB/s
        {
            return $"{speedInKBps / 1048576.0:F2} GB/s";  // 显示为GB/s
        }
        else if (speedInKBps >= 1024)  // 如果速度超过1024 KB/s，即超过1 MB/s
        {
            return $"{speedInKBps / 1024.0:F2} MB/s";  // 显示为MB/s
        }
        else
        {
            return $"{speedInKBps:F2} KB/s";  // 显示为KB/s
        }
    }


    // protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    // {
    //     // 取消关闭事件，使得窗口不会真的被关闭
    //     e.Cancel = true;
    //     this.Hide();
    //     ((MainViewModel)DataContext).IsWindowVisible = false;
    //     base.OnClosing(e);
    // }


    private void InitializeNetworkInterface()
    {
        // 这里简单地选取第一个活动的网络接口
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                lastBytesSent = ni.GetIPv4Statistics().BytesSent;
                lastBytesReceived = ni.GetIPv4Statistics().BytesReceived;
                break;
            }
        }
    }
    // protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    // {
    //     // 取消关闭事件，使得窗口不会真的被关闭
    //     e.Cancel = true;
    //     this.Hide(); // 隐藏窗口
    //     base.OnClosing(e);
    // }


    private void OpenCommandExecuted()
    {
        Debug.WriteLine("Open command executed"); // 或使用 Debug.WriteLine 如果在 GUI 应用中
        this.Show();
        this.WindowState = WindowState.Normal;
    }

    private void ExitCommandExecuted()
    {
        Debug.WriteLine("Exit command executed");
        Application.Current.Shutdown();
    }

    private void StartMonitoring()
    {
        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    // private void Timer_Tick(object sender, EventArgs e)
    // {
    //     NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
    //     foreach (NetworkInterface ni in interfaces)
    //     {
    //         if (ni.OperationalStatus == OperationalStatus.Up &&
    //             ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
    //         {
    //             long bytesSent = ni.GetIPv4Statistics().BytesSent;
    //             long bytesReceived = ni.GetIPv4Statistics().BytesReceived;
    //
    //             long sentSpeed = bytesSent - lastBytesSent;
    //             long receivedSpeed = bytesReceived - lastBytesReceived;
    //
    //             lastBytesSent = bytesSent;
    //             lastBytesReceived = bytesReceived;
    //
    //             // Update the UI
    //             uploadSpeedText.Text = FormatSpeed(sentSpeed);
    //             downloadSpeedText.Text = FormatSpeed(receivedSpeed);
    //             break;
    //         }
    //     }
    // }

// Utility method to format speed
    // private string FormatSpeed(long speed)
    // {
    //     // Convert to MB/s if the speed is more than 1 MB
    //     if (speed >= 1048576)
    //     {
    //         return $"{(double)speed / 1048576.0:N2} MB/s";
    //     }
    //
    //     return $"{(double)speed / 1024.0:N2} KB/s";
    // }
    //


    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // 双击鼠标左键时关闭应用程序
        if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
        {
            ((MainViewModel)DataContext).IsWindowVisible = false;
            Hide(); // 关闭当前窗口，这会退出应用程序
        }
    }
    
    
    private void Grid_DragEnter(object sender, DragEventArgs e)
    {
        // 判断是否是文件类型
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }
    
    

    private void Grid_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                
                string result = string.Join(Environment.NewLine, files);
                Clipboard.SetText(result);
                // 获取当前窗口的位置信息
                // var currentLeft = this.Left-15;
                // var currentTop = this.Top-6;
                var currentLeft = this.Left;
                var currentTop = this.Top;
                var currentWidth = this.Width;
                var currentHeight = this.Height;
                // 创建新窗口
                var newWindow = new FaceWindow(); // 你可以换成别的 Window 类型
                // 设置位置和大小一致
                newWindow.Left = currentLeft;
                newWindow.Top = currentTop;
                newWindow.Width = currentWidth;
                newWindow.Height = currentHeight;
                // 如果你希望它在当前窗口上方（Z序更高）
                newWindow.Topmost = true;
                this.Hide();
                // 显示窗口
                newWindow.Show();
                newWindow.AnimateFace(this);
                Point dropPosition = e.GetPosition(FileCanvas); // 鼠标拖入点
                newWindow.AnimateFlyingFile(dropPosition);
                // this.Show();
            
            }
        }
    }
   




    public void ShowWindow()
    {
        OpenCommandExecuted();
    }

    public void HideWindow()
    {
        Hide();
    }
}