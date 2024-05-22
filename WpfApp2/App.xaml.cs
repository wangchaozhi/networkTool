using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using Newtonsoft.Json;
using WpfApp2.Windows;

namespace WpfApp2;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private TaskbarIcon notifyIcon;


    // 在类中声明这些 API
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}"); // 使用一个唯一的标识符
    private bool hasMutex = false; // 添加字段来跟踪互斥锁的拥有权

    private void InitializeAutoStartMenuItem()
    {
        string appName = "MyApp";
        try
        {
            using (RegistryKey key =
                   Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                bool isAutoStartEnabled = key != null && key.GetValue(appName) != null;
                // var autoStartMenu = ((ContextMenu)notifyIcon.ContextMenu).Items[0] as MenuItem; // 确保Items索引与实际对应
                // var autoStartMenu = autoStartMenu;
                var contextMenu = notifyIcon.ContextMenu as ContextMenu;
                var autoStartMenu = contextMenu.Items
                    .OfType<MenuItem>()
                    .FirstOrDefault(item => item.Header.ToString() == "开机自启");
                autoStartMenu.IsChecked = isAutoStartEnabled;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize auto-start settings: {ex.Message}");
        }
    }


    protected override void OnStartup(StartupEventArgs e)
    {
        // if (!mutex.WaitOne(TimeSpan.Zero, true))
        // {
        //     // 如果已有一个实例在运行，就提示用户并关闭此实例
        //     MessageBox.Show("应用程序已经在运行。");
        //     Application.Current.Shutdown();  // 关闭当前应用程序实例
        //     return;
        // }

        hasMutex = mutex.WaitOne(TimeSpan.Zero, true);
        if (!hasMutex)
        {
            // 如果获取互斥锁失败，则可能已有实例在运行
            IntPtr hWnd = FindWindow(null, "MainWindow"); // 确保窗口标题正确
            if (hWnd != IntPtr.Zero)
            {
                SetForegroundWindow(hWnd); // 将已运行的实例窗口带到前台
                // 也可以选择显示或隐藏窗口
                // ShowWindow(hWnd, 5);  // SW_SHOW
                // ShowWindow(hWnd, 0);  // SW_HIDE
            }

            Application.Current.Shutdown(); // 关闭当前应用程序实例
            return;
        }


        base.OnStartup(e);
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;

        var windowService = new WindowService(mainWindow);
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appDirectory = Path.Combine(appDataPath, "MyApp");
        string jsonFilePath = Path.Combine(appDirectory, "config.json");

        if (!Directory.Exists(appDirectory))
        {
            Directory.CreateDirectory(appDirectory);
        }

        var configurationManager = new ConfigurationManager(jsonFilePath);
        var viewModel = new MainViewModel(windowService, configurationManager);

        MainWindow.DataContext = viewModel;

        notifyIcon = (TaskbarIcon)FindResource("MyNotifyIcon");
        notifyIcon.DataContext = viewModel;
        // 注册事件

        var autoStartMenu = ((ContextMenu)notifyIcon.ContextMenu).Items
            .OfType<MenuItem>()
            .FirstOrDefault(item => item.Header.ToString() == "开机自启");


        // var autoStartMenu = ((ContextMenu)notifyIcon.ContextMenu).Items[1] as MenuItem;
        autoStartMenu.Checked += AutoStart_Checked;
        autoStartMenu.Unchecked += AutoStart_Unchecked;
        notifyIcon.TrayMouseDoubleClick += NotifyIcon_TrayMouseDoubleClick;
        InitializeAutoStartMenuItem();

        MainWindow.Show();
        // MainWindow.Hide(); // 根据需要取消注释
    }


    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}", "Error", MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true; // Prevent the application from crashing
    }

    private void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"A critical unhandled exception occurred: {e.ExceptionObject}", "Critical Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
        // Consider logging the exception and shutting down the application
    }


    private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        // 检查 MainWindow 是否存在并且 DataContext 是 MainViewModel
        if (MainWindow != null && MainWindow.DataContext is MainViewModel viewModel)
        {
            if (MainWindow.IsVisible)
            {
                // 如果窗口正在显示，则隐藏它
                viewModel.IsWindowVisible = false;
                MainWindow.Hide();
            }
            else
            {
                // 如果窗口是隐藏的，设置 IsWindowVisible 为 true 并显示窗口
                viewModel.IsWindowVisible = true;
                MainWindow.Show();
                MainWindow.WindowState = WindowState.Normal;
            }
        }
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        ContextMenu menu = sender as ContextMenu;
        if (menu != null)
        {
            POINT p;
            if (GetCursorPos(out p))
            {
                // 将鼠标位置转换为 DPI 无关的值
                var visual = Application.Current.MainWindow as Visual;
                double dpiFactor = VisualTreeHelper.GetDpi(visual).DpiScaleX;
                menu.HorizontalOffset = p.X / dpiFactor;
                menu.VerticalOffset = p.Y / dpiFactor;
                menu.Placement = System.Windows.Controls.Primitives.PlacementMode.AbsolutePoint;
            }
        }
    }

    private void AutoStart_Checked(object sender, RoutedEventArgs e)
    {
        SetApplicationToRunAtStartup();
    }

    private void AutoStart_Unchecked(object sender, RoutedEventArgs e)
    {
        RemoveApplicationFromStartup();
    }

    private void SetApplicationToRunAtStartup()
    {
        string appName = "MyApp";
        string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        // 确认路径以.dll结尾，然后替换为.exe
        if (appPath.EndsWith(".dll"))
        {
            appPath = appPath.Replace(".dll", ".exe");
        }

        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        key.SetValue(appName, $"\"{appPath}\"");
    }

    private void RemoveApplicationFromStartup()
    {
        string appName = "MyApp";
        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        key.DeleteValue(appName, false);
    }

    private void Update_Click(object sender, RoutedEventArgs e)
    {
        // 调用更新逻辑
        CheckForUpdates();
    }

    private async void CheckForUpdates()
    {
        try
        {
            // 发送 GET 请求到更新服务器
            using (var client = new HttpClient())
            {
                var configurationManager = MainViewModel._configManager;
                configurationManager.SetVersion("v0.4");
                var version = configurationManager.GetVersion();
                // string updateCheckUrl = $"http://192.168.3.23:3000/api/check-for-updates?version={version}"; // 更新检查 API 地址
                string updateCheckUrl = $"http://8.134.168.19:3000/api/check-for-updates?version={version}"; // 更新检查 API 地址
                // string updateCheckUrl = "http://192.168.3.23:3000/api/check-for-updates"; // 更新检查 API 地址
                HttpResponseMessage response = await client.GetAsync(updateCheckUrl);
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // 解析 JSON 响应
                UpdateInfo updateInfo = JsonConvert.DeserializeObject<UpdateInfo>(jsonResponse);
                
                if (updateInfo != null && updateInfo.UpdateAvailable)
                {
                    
                    
                    UpdateDialog dialog = new UpdateDialog();
                    bool? result = dialog.ShowDialog();

                    if (result.HasValue && result.Value)
                    {
                        // 用户点击了 "是" 按钮，执行更新逻辑
                        StartUpdateProcess(updateInfo.Url, updateInfo.Filename);
                    }
                }
                else
                {
                    var tipDialog = new TipDialog("您的程序已是最新版本。");
                    tipDialog.ShowDialog();
                }
            }
        }
        catch (Exception ex)
        {
            var dialog = new TipDialog("连接更新服务器失败,网络出小差了~~");
            dialog.Show();
        }
    }

    private void StartUpdateProcess(string url, string filename)
    {
        try
        {
            // 获得当前执行文件的目录
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            string updaterPath = Path.Combine(directory, "WpfUpdate.exe");

            // 构建命令行参数
            string arguments = $"-u \"{url}\" -f \"{filename}\"";

            // 启动更新程序1
            Process.Start(updaterPath, arguments);
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
            var dialog = new TipDialog($"更新服务启动失败！");
            dialog.Show();
        }
    }


    public class UpdateInfo
    {
        public bool UpdateAvailable { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
    }


    protected override void OnExit(ExitEventArgs e)
    {
        if (hasMutex)
        {
            mutex.ReleaseMutex(); // 只有在成功获取互斥锁时才释放它
        }

        if (notifyIcon != null)
        {
            notifyIcon.Dispose(); // 清理托盘图标，避免残留
        }

        base.OnExit(e);
    }
}