using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;


namespace WpfApp2;

public partial class MainWindow : Window,IWindowService
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
        InitializeNetworkInterface();
        StartMonitoring();
        
        var viewModel = new MainViewModel(this);
        // 监听 ViewModel 中的窗口显示状态变化
        this.DataContext = viewModel;
        // 将 TaskbarIcon 的 DataContext 设置为 ViewModel
        var notifyIcon = (TaskbarIcon)Application.Current.FindResource("MyNotifyIcon");
        notifyIcon.DataContext = viewModel;
    }

 

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // 取消关闭事件，使得窗口不会真的被关闭
        e.Cancel = true;
        ((MainViewModel)DataContext).IsWindowVisible = false;
        base.OnClosing(e);
    }
    
   

    

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
}