using System.Windows;

namespace WpfApp2.Windows;

public partial class TipDialog : Window
{
    public string Message { get; set; }
    public TipDialog()
    {
        InitializeComponent();
    }
    
    public TipDialog(string message)
    {
        InitializeComponent();
        Message = message;
        DataContext = this;  // 设置 DataContext 以支持数据绑定
    }
    
    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
    // private void YesButton_Click(object sender, RoutedEventArgs e)
    // {
    //     DialogResult = true;  // 设置对话框的结果为 true 并关闭窗口
    // }
    
}