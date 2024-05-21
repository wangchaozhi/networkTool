using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfApp2;

public partial class TaiChiWindow : Window, INotifyPropertyChanged
{
    private double _currentScale = 1;
    public double CurrentScale
    {
        get => _currentScale;
        set
        {
            if (_currentScale != value)
            {
                _currentScale = value;
                OnPropertyChanged(nameof(CurrentScale));
            }
        }
    }

    public TaiChiWindow()
    {
        InitializeComponent();
        this.DataContext = this;
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Taichi.MouseDown += Taichi_MouseDown; // 注册点击事件
    }

    private void Taichi_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // 取消事件订阅，防止动画运行期间重复触发
        Taichi.MouseDown -= Taichi_MouseDown;
        StartTaiChiAnimation();
    }

    private void StartTaiChiAnimation()
    {
        DoubleAnimation rotateAnimation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = new Duration(TimeSpan.FromSeconds(5)),
            RepeatBehavior = new RepeatBehavior(10), // 让动画只运行一次
            AccelerationRatio = 0.2,
            DecelerationRatio = 0.5
        };
        rotateAnimation.Completed += Animation_Completed; // 动画完成后的事件

        RotateTransform rotateTransform = new RotateTransform();
        if (Taichi.RenderTransform is TransformGroup transformGroup)
        {
            var existingRotateTransform = transformGroup.Children.OfType<RotateTransform>().FirstOrDefault();
            if (existingRotateTransform != null)
            {
                rotateTransform = existingRotateTransform;
            }
            else
            {
                transformGroup.Children.Add(rotateTransform);
            }
        }
        else
        {
            Taichi.RenderTransform = new TransformGroup { Children = new TransformCollection { rotateTransform }};
        }

        rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
    }

    private void Animation_Completed(object sender, EventArgs e)
    {
        // 动画完成后重新订阅事件
        Taichi.MouseDown += Taichi_MouseDown;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        var windowWidth = this.ActualWidth;
        var windowHeight = this.ActualHeight;

        this.Left = (screenWidth / 2) - (windowWidth / 2);
        this.Top = (screenHeight / 2) - (windowHeight / 2);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
