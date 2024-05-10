using System;
using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;

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
    
    // private void InitializeAutoStartMenuItem()
    // {
    //     string appName = "MyApp";
    //     RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
    //     bool isAutoStartEnabled = key.GetValue(appName) != null;
    //
    //     var autoStartMenu = ((ContextMenu)notifyIcon.ContextMenu).Items[0] as MenuItem; // 确保Items索引与实际对应
    //     autoStartMenu.IsChecked = isAutoStartEnabled;
    // }
    
    
    // private void InitializeAutoStartMenuItem()
    // {
    //     string appName = "MyApp";
    //     using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
    //     {
    //         bool isAutoStartEnabled = key != null && key.GetValue(appName) != null;
    //         var autoStartMenu = ((ContextMenu)notifyIcon.ContextMenu).Items[0] as MenuItem; // 确保Items索引与实际对应
    //         autoStartMenu.IsChecked = isAutoStartEnabled;
    //     }
    // }
    
    
    private void InitializeAutoStartMenuItem()
    {
        string appName = "MyApp";
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                bool isAutoStartEnabled = key != null && key.GetValue(appName) != null;
                var autoStartMenu = ((ContextMenu)notifyIcon.ContextMenu).Items[0] as MenuItem; // 确保Items索引与实际对应
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
        base.OnStartup(e);
    
        var mainWindow = new MainWindow();
        MainWindow = mainWindow;

        var windowService = new WindowService(mainWindow);
        var viewModel = new MainViewModel(windowService);
    
        MainWindow.DataContext = viewModel;

        notifyIcon = (TaskbarIcon)FindResource("MyNotifyIcon");
        notifyIcon.DataContext = viewModel;
        // 注册事件
        var autoStartMenu = ((ContextMenu)notifyIcon.ContextMenu).Items[1] as MenuItem;
        autoStartMenu.Checked += AutoStart_Checked;
        autoStartMenu.Unchecked += AutoStart_Unchecked;
        notifyIcon.TrayMouseDoubleClick += NotifyIcon_TrayMouseDoubleClick;
        InitializeAutoStartMenuItem();
    
        MainWindow.Show();
        // MainWindow.Hide(); // 根据需要取消注释
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

    
    protected override void OnExit(ExitEventArgs e)
    {
        if (notifyIcon != null)
        {
            notifyIcon.Dispose(); // 清理托盘图标，避免残留
        }
        base.OnExit(e);
    }

}