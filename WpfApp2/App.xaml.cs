using System;
using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;

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

    // protected override void OnStartup(StartupEventArgs e)
    // {
    //     // base.OnStartup(e);
    //     //
    //     // // 初始化托盘图标
    //     // notifyIcon = (TaskbarIcon)FindResource("MyNotifyIcon");
    //     // notifyIcon.TrayMouseDoubleClick += NotifyIcon_TrayMouseDoubleClick;
    //     // // 显示主窗口
    //     // // MainWindow = new MainWindow();
    //     // // MainWindow.Show();
    //     // // MainWindow.Hide(); // 启动时隐藏窗口
    //     
    //     
    //     base.OnStartup(e);
    //
    //     // 从资源中获取托盘图标实例并设置
    //     notifyIcon = (TaskbarIcon)FindResource("MyNotifyIcon");
    //     notifyIcon.TrayMouseDoubleClick += NotifyIcon_TrayMouseDoubleClick;
    //     var mainWindow = new MainWindow();
    //
    //     // 设置托盘图标的 DataContext
    //     var viewModel = new MainViewModel(mainWindow);
    //     notifyIcon.DataContext = viewModel;
    //     // 初始化并显示主窗口
    //     MainWindow = mainWindow;
    //     MainWindow.DataContext = viewModel; // 确保主窗口也使用相同的 ViewModel
    //     MainWindow.Show();
    //     // MainWindow.Hide(); // 启动时隐藏主窗口
    // }
    
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
        notifyIcon.TrayMouseDoubleClick += NotifyIcon_TrayMouseDoubleClick;
    
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




    protected override void OnExit(ExitEventArgs e)
    {
        notifyIcon.Dispose(); // 清理托盘图标
        base.OnExit(e);
    }
}