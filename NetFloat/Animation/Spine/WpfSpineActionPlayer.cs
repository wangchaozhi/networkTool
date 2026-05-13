using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NetFloat.Animation.Spine;

public sealed class WpfSpineActionPlayer : ISpineAnimationPlayer
{
    private const int FileEatFrameCount = 4;
    private const double FileSize = 54;
    private const double TravelDistance = 132;
    private static readonly TimeSpan FlyDuration = TimeSpan.FromMilliseconds(560);
    private static readonly TimeSpan CleanupDelay = TimeSpan.FromMilliseconds(1240);

    private readonly Window _hostWindow;
    private readonly Image _openCharacterImage;
    private readonly Image _closedCharacterImage;
    private readonly Canvas _effectCanvas;
    private BitmapSource[]? _fileEatFrames;

    public WpfSpineActionPlayer(
        Window hostWindow,
        Image openCharacterImage,
        Image closedCharacterImage,
        Canvas effectCanvas)
    {
        _hostWindow = hostWindow;
        _openCharacterImage = openCharacterImage;
        _closedCharacterImage = closedCharacterImage;
        _effectCanvas = effectCanvas;
    }

    public void Play(SpineActionName actionName, SpineActionRequest request)
    {
        if (actionName != SpineActionName.EatFile)
        {
            throw new NotSupportedException($"Spine action '{actionName}' is not implemented yet.");
        }

        PlayEatFile(request);
    }

    private void PlayEatFile(SpineActionRequest request)
    {
        _hostWindow.UpdateLayout();
        _openCharacterImage.Opacity = 1;
        _closedCharacterImage.Opacity = 0;
        PrepareCharacterTransform();

        var dragDirection = NormalizeDragDirection(request.DragDirection);
        var mouthPoint = GetMouthPoint();
        var startPoint = GetDirectionalStartPoint(request.DropPoint, mouthPoint, dragDirection);
        var rotationAngle = Math.Atan2(dragDirection.Y, dragDirection.X) * 180 / Math.PI;

        var fileImage = CreateFileImage(rotationAngle);
        Canvas.SetLeft(fileImage, startPoint.X - FileSize / 2);
        Canvas.SetTop(fileImage, startPoint.Y - FileSize / 2);
        _effectCanvas.Children.Add(fileImage);

        AnimateFile(fileImage, startPoint, mouthPoint);
        PlayBiteFrames(fileImage);
        CompleteAfterDelay(fileImage, request.Completed);
    }

    private static Vector NormalizeDragDirection(Vector dragDirection)
    {
        if (dragDirection.Length < 1)
        {
            return new Vector(1, 0);
        }

        dragDirection.Normalize();
        return dragDirection;
    }

    private void PrepareCharacterTransform()
    {
        var openTransform = CreateCharacterTransform();
        var closedTransform = CreateCharacterTransform();
        _openCharacterImage.RenderTransformOrigin = new Point(0.5, 0.68);
        _closedCharacterImage.RenderTransformOrigin = new Point(0.5, 0.68);
        _openCharacterImage.RenderTransform = openTransform;
        _closedCharacterImage.RenderTransform = closedTransform;
    }

    private static TransformGroup CreateCharacterTransform()
    {
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(new RotateTransform(0));
        transformGroup.Children.Add(new TranslateTransform(0, 0));
        return transformGroup;
    }

    private Point GetMouthPoint()
    {
        return _openCharacterImage.TransformToAncestor(_hostWindow).Transform(
            new Point(_openCharacterImage.ActualWidth * 0.34, _openCharacterImage.ActualHeight * 0.42));
    }

    private Image CreateFileImage(double rotationAngle)
    {
        return new Image
        {
            Width = FileSize,
            Height = FileSize,
            Opacity = 0,
            Source = GetFileEatFrames()[0],
            RenderTransform = new RotateTransform(rotationAngle),
            RenderTransformOrigin = new Point(0.5, 0.5)
        };
    }

    private void AnimateFile(Image fileImage, Point startPoint, Point mouthPoint)
    {
        var easing = new CubicEase { EasingMode = EasingMode.EaseIn };
        var leftAnim = new DoubleAnimation(startPoint.X - FileSize / 2, mouthPoint.X - FileSize / 2, FlyDuration)
        {
            EasingFunction = easing
        };
        var topAnim = new DoubleAnimation(startPoint.Y - FileSize / 2, mouthPoint.Y - FileSize / 2, FlyDuration)
        {
            EasingFunction = easing
        };
        var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(120));

        fileImage.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
        fileImage.BeginAnimation(Canvas.LeftProperty, leftAnim);
        fileImage.BeginAnimation(Canvas.TopProperty, topAnim);
    }

    private void PlayBiteFrames(Image fileImage)
    {
        var frames = GetFileEatFrames();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80) };
        var step = 0;
        timer.Tick += (_, _) =>
        {
            step++;
            switch (step)
            {
                case 6:
                    fileImage.Source = frames[1];
                    CloseMouth();
                    break;
                case 8:
                    fileImage.Source = frames[2];
                    OpenMouth();
                    break;
                case 10:
                    fileImage.Source = frames[3];
                    CloseMouth();
                    break;
                case 12:
                    fileImage.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(120)));
                    OpenMouth();
                    timer.Stop();
                    break;
            }
        };
        timer.Start();
    }

    private void CompleteAfterDelay(UIElement fileImage, Action? completed)
    {
        var timer = new DispatcherTimer { Interval = CleanupDelay };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            _effectCanvas.Children.Remove(fileImage);
            _openCharacterImage.Opacity = 0;
            _closedCharacterImage.Opacity = 0;
            completed?.Invoke();
        };
        timer.Start();
    }

    private void CloseMouth()
    {
        _openCharacterImage.Opacity = 0;
        _closedCharacterImage.Opacity = 1;
    }

    private void OpenMouth()
    {
        _openCharacterImage.Opacity = 1;
        _closedCharacterImage.Opacity = 0;
    }

    private BitmapSource[] GetFileEatFrames()
    {
        if (_fileEatFrames is not null)
        {
            return _fileEatFrames;
        }

        var sheet = new BitmapImage(new Uri("pack://application:,,,/res/file_eaten_sheet.png"));
        var frameWidth = sheet.PixelWidth / FileEatFrameCount;
        var frameHeight = sheet.PixelHeight;
        _fileEatFrames = new BitmapSource[FileEatFrameCount];

        for (var i = 0; i < FileEatFrameCount; i++)
        {
            _fileEatFrames[i] = new CroppedBitmap(
                sheet,
                new Int32Rect(i * frameWidth, 0, frameWidth, frameHeight));
        }

        return _fileEatFrames;
    }

    private Point GetDirectionalStartPoint(Point dropPoint, Point mouthPoint, Vector dragDirection)
    {
        var directionalPoint = mouthPoint - dragDirection * TravelDistance;

        if (IsInsideWindow(dropPoint) && (dropPoint - mouthPoint).Length > 48)
        {
            directionalPoint = new Point(
                (directionalPoint.X + dropPoint.X) / 2,
                (directionalPoint.Y + dropPoint.Y) / 2);
        }

        return directionalPoint;
    }

    private bool IsInsideWindow(Point point)
    {
        return point.X >= -24 && point.X <= _hostWindow.ActualWidth + 24 &&
               point.Y >= -24 && point.Y <= _hostWindow.ActualHeight + 24;
    }
}
