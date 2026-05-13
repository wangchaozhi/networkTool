using System;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NetFloat.Windows;

namespace NetFloat;

public partial class MainWindow
{
    private long _lastBytesSent;
    private long _lastBytesReceived;
    private DispatcherTimer? _timer;
    private NetworkInterface? _currentInterface;
    private Point? _lastDragPoint;
    private Point? _latestDragPoint;

    private const long KiloByte = 1024;
    private const long MegaByte = 1024 * KiloByte;
    private const long GigaByte = 1024 * MegaByte;

    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;
        this.Closed += MainWindow_Closed;
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        NetworkChange.NetworkAddressChanged -= NetworkAddressChanged;
        NetworkChange.NetworkAvailabilityChanged -= NetworkAvailabilityChanged;
        this.Closed -= MainWindow_Closed;
    }

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        double screenWidth = SystemParameters.WorkArea.Width;
        double screenHeight = SystemParameters.WorkArea.Height;
        this.Left = screenWidth / 1.15;
        this.Top = screenHeight / 22;

        NetworkChange.NetworkAddressChanged += NetworkAddressChanged;
        NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChanged;
        InitializeNetworkInterface();
    }

    private void NetworkAddressChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(InitializeNetworkInterface);
    }

    private void NetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
    {
        if (e.IsAvailable)
            Dispatcher.Invoke(InitializeNetworkInterface);
    }

    private void InitializeNetworkInterface()
    {
        NetworkInterface? best = null;
        long maxData = 0;

        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Ppp) continue;
            string desc = ni.Description.ToLowerInvariant();
            if (desc.Contains("virtual") || desc.Contains("vpn") ||
                desc.Contains("pseudo") || desc.Contains("vmware")) continue;

            IPv4InterfaceStatistics stats = ni.GetIPv4Statistics();
            long totalData = stats.BytesSent + stats.BytesReceived;
            if (totalData > maxData)
            {
                best = ni;
                maxData = totalData;
            }
        }

        if (best == null) return;

        _currentInterface = best;
        var initial = _currentInterface.GetIPv4Statistics();
        _lastBytesSent = initial.BytesSent;
        _lastBytesReceived = initial.BytesReceived;
        StartMonitoring();
    }

    private void StartMonitoring()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
        }

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_currentInterface == null) return;

        var stats = _currentInterface.GetIPv4Statistics();
        long bytesSent = stats.BytesSent;
        long bytesReceived = stats.BytesReceived;

        long sentSpeed = bytesSent - _lastBytesSent;
        long receivedSpeed = bytesReceived - _lastBytesReceived;

        if (sentSpeed < 0 || receivedSpeed < 0)
        {
            sentSpeed = 0;
            receivedSpeed = 0;
        }

        _lastBytesSent = bytesSent;
        _lastBytesReceived = bytesReceived;

        uploadSpeedText.Text = FormatSpeed(sentSpeed);
        downloadSpeedText.Text = FormatSpeed(receivedSpeed);
    }

    private string FormatSpeed(long bytes)
    {
        if (bytes >= GigaByte) return $"{(double)bytes / GigaByte:F2} GB/s";
        if (bytes >= MegaByte) return $"{(double)bytes / MegaByte:F2} MB/s";
        return $"{(double)bytes / KiloByte:F2} KB/s";
    }

    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            try { this.DragMove(); }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }

        if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.IsWindowVisible = false;
            }
            Hide();
        }
    }

    private void Grid_DragEnter(object sender, DragEventArgs e)
    {
        var position = e.GetPosition(FileCanvas);
        _lastDragPoint = position;
        _latestDragPoint = position;
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void Grid_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        _lastDragPoint = _latestDragPoint;
        _latestDragPoint = e.GetPosition(FileCanvas);
        e.Effects = DragDropEffects.Copy;
    }

    private void Grid_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files)
        {
            return;
        }

        if (files.Length == 0) return;

        Clipboard.SetText(string.Join(Environment.NewLine, files));

        const double animationWidth = 170;
        const double animationHeight = 160;
        var dropPoint = e.GetPosition(FileCanvas);
        var dragDirection = GetDragDirection(dropPoint);

        var newWindow = new FaceWindow
        {
            Left = this.Left + (this.Width - animationWidth) / 2,
            Top = this.Top + (this.Height - animationHeight) / 2,
            Width = animationWidth,
            Height = animationHeight,
            Topmost = true
        };
        this.Hide();
        newWindow.Show();
        var animationDropPoint = new Point(dropPoint.X + this.Left - newWindow.Left, dropPoint.Y + this.Top - newWindow.Top);
        newWindow.PlayEatFile(this, animationDropPoint, dragDirection);
        _lastDragPoint = null;
        _latestDragPoint = null;
    }

    private Vector GetDragDirection(Point dropPoint)
    {
        if (_lastDragPoint is { } lastPoint && _latestDragPoint is { } latestPoint)
        {
            var dragDirection = latestPoint - lastPoint;
            if (dragDirection.Length >= 1)
            {
                return dragDirection;
            }
        }

        if (_latestDragPoint is { } latestDragPoint)
        {
            var dragDirection = dropPoint - latestDragPoint;
            if (dragDirection.Length >= 1)
            {
                return dragDirection;
            }
        }

        return new Vector(1, 0);
    }

    public void ShowWindow()
    {
        this.Show();
        this.WindowState = WindowState.Normal;
    }

    public void HideWindow() => Hide();
}
