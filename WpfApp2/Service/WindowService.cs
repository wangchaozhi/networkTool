using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace WpfApp2;

public class WindowService : IWindowService
{
    private readonly MainWindow _mainWindow;

    public WindowService(MainWindow mainWindow)
    {
        this._mainWindow = mainWindow;
    }

    public void ShowWindow()
    {
        _mainWindow.Show();
    }

    public void HideWindow()
    {
        _mainWindow.Hide();
    }

    public void ResizeWindow(double width, double height)
    {
        _mainWindow.Width = width;
        _mainWindow.Height = height;
    }

 
    
    public void ChangeIconSource(IconData iconData)
    {
        _mainWindow.Dispatcher.Invoke(() =>
        {
            var uploadSpeedImage = (Image)_mainWindow.FindName("uploadSpeedImage");
            var downloadSpeedImage = (Image)_mainWindow.FindName("downloadSpeedImage");

            if (uploadSpeedImage != null && downloadSpeedImage != null)
            {
                uploadSpeedImage.Source = new BitmapImage(new Uri(iconData.UpIconPath));
                downloadSpeedImage.Source = new BitmapImage(new Uri(iconData.DownIconPath));
            }
        });
    }

}