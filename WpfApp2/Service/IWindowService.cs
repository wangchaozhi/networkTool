namespace WpfApp2;

public interface IWindowService
{
    void ShowWindow();
    void HideWindow();


    void ResizeWindow(double width, double height);


    void ChangeIconSource(IconData iconData);


}
