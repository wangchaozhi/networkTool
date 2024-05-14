namespace WpfApp2;

public class WindowService : IWindowService
{
    private readonly MainWindow mainWindow;

    public WindowService(MainWindow mainWindow)
    {
        this.mainWindow = mainWindow;
    }

    public void ShowWindow()
    {
        mainWindow.Show();
    }

    public void HideWindow()
    {
        mainWindow.Hide();
    }

    public void ResizeWindow(double width, double height)
    {
        mainWindow.Width = width;
        mainWindow.Height = height;
    }
}