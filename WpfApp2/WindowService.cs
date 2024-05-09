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
}