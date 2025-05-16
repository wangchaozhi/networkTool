using System.Windows;

namespace WpfApp2.Windows;


    public partial class UpdateDialog : Window
    {
        public bool? UpdateNow { get; set; }

        public UpdateDialog()
        {
            InitializeComponent();
        }

        // private void YesButton_Click(object sender, RoutedEventArgs e)
        // {
        //     UpdateNow = true;
        //     Close();
        // }
        //
        // private void NoButton_Click(object sender, RoutedEventArgs e)
        // {
        //     UpdateNow = false;
        //     Close();
        // }
        
        
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // 设置对话框的结果为 true
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // 设置对话框的结果为 false
            this.Close();
        }
    }
