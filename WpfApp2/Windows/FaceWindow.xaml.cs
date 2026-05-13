using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfApp2.Windows;

public partial class FaceWindow : Window
{
    public FaceWindow()
    {
        InitializeComponent();
    }
    
  
    public void AnimateFace(Window that)
    {
        // 让脸出现
        FaceImage.Opacity = 1;

        // 创建放大缩小动画（类似吞的动作）
        var scale = new ScaleTransform(1.0, 1.0);
        FaceImage.RenderTransformOrigin = new Point(0.5, 0.5);
        FaceImage.RenderTransform = scale;
        
        var animation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.3,
            Duration = TimeSpan.FromMilliseconds(200),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(2)
        };
        
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        // 1 秒后隐藏大脸
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (s, e) =>
        {
            FaceImage.Opacity = 0;
            timer.Stop();
            // 👇 关闭窗口
            this.Close();
            that.Show();
        };
        timer.Start();
    }
    
    
    public void AnimateFlyingFile(Point startPoint)
    {
        // 创建文件图标（用图片或用📄 Emoji都行）
        var fileImage = new Image
        {
            Width = 64,
            Height = 64,
            Source = new BitmapImage(new Uri("pack://application:,,,/res/file.png")) // 替换你的图标路径
            
        };

        // 设置初始位置
        Canvas.SetLeft(fileImage, startPoint.X - 32); // 居中
        Canvas.SetTop(fileImage, startPoint.Y - 32);
        FileCanvas.Children.Add(fileImage);

        // 目标位置（大脸中心）
        Point targetPoint = FaceImage.TransformToAncestor(this).Transform(new Point(FaceImage.ActualWidth / 2, FaceImage.ActualHeight / 2));

        // 位移动画
        var leftAnim = new DoubleAnimation(startPoint.X - 32, targetPoint.X - 32, TimeSpan.FromMilliseconds(700));
        var topAnim = new DoubleAnimation(startPoint.Y - 32, targetPoint.Y - 32, TimeSpan.FromMilliseconds(700));

        // 缩小动画
        var scale = new ScaleTransform(1.0, 1.0);
        fileImage.RenderTransform = scale;
        fileImage.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnim = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(700));

        // 同时执行
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        fileImage.BeginAnimation(Canvas.LeftProperty, leftAnim);
        fileImage.BeginAnimation(Canvas.TopProperty, topAnim);

        // 动画结束后移除图像
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(750) };
        timer.Tick += (s, e) =>
        {
            FileCanvas.Children.Remove(fileImage);
            timer.Stop();
        };
        timer.Start();
    }
}