using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;


namespace WpfApp2;

public partial class MainWindow : IWindowService
{
    private long lastBytesSent = 0;
    private long lastBytesReceived = 0;
    private DispatcherTimer timer;
    private TaskbarIcon notifyIcon;
    // public ICommand OpenCommand { get; private set; }
    // public ICommand ExitCommand { get; private set; }
    
    public MainWindow()
    {
        InitializeComponent();
        // GlassWindowManager.EnableGlassEffect(this);
        // 注册窗体关闭事件以清理资源
        // this.Closed += MainWindow_Closed;
        // this.SourceInitialized += MainWindow_SourceInitialized;
        // 设置窗口启动位置
        this.Loaded += MainWindow_Loaded;
        InitializeNetworkInterface();
        StartMonitoring();
      
    }
    
    private void MainWindow_Closed(object sender, EventArgs e)
    {
        // 在窗体关闭时注销事件处理程序
        this.Closed -= MainWindow_Closed;
    }
    
    private void MainWindow_SourceInitialized(object sender, EventArgs e)
    {
        var hWnd = new WindowInteropHelper(this).Handle;
        EnableGlassEffect(hWnd);
    }

    private void EnableGlassEffect(IntPtr hWnd)
    {
        var margins = new GlassEffect.MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
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
    
    
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 获取屏幕工作区域的宽度和高度
        double screenWidth = SystemParameters.WorkArea.Width;
        double screenHeight = SystemParameters.WorkArea.Height;

        // 设置窗口在屏幕右上角
        this.Left = screenWidth/1.15;
        this.Top = screenHeight/22;
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
        Debug.WriteLine("Open command executed");  // 或使用 Debug.WriteLine 如果在 GUI 应用中
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

    private void Timer_Tick(object sender, EventArgs e)
    {
        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface ni in interfaces)
        {
            if (ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                long bytesSent = ni.GetIPv4Statistics().BytesSent;
                long bytesReceived = ni.GetIPv4Statistics().BytesReceived;

                long sentSpeed = bytesSent - lastBytesSent;
                long receivedSpeed = bytesReceived - lastBytesReceived;

                lastBytesSent = bytesSent;
                lastBytesReceived = bytesReceived;

                // Update the UI
                uploadSpeedText.Text = FormatSpeed(sentSpeed);
                downloadSpeedText.Text = FormatSpeed(receivedSpeed);
                break;
            }
        }
    }

// Utility method to format speed
    private string FormatSpeed(long speed)
    {
        // Convert to MB/s if the speed is more than 1 MB
        if (speed >= 1048576)
        {
            return $"{(double)speed / 1048576.0:N2} MB/s";
        }

        return $"{(double)speed / 1024.0:N2} KB/s";
    }
    
    
    
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


    public void ShowWindow()
    {
        OpenCommandExecuted();
    }

    public void HideWindow()
    {
        Hide();
    }
}