using System.Windows;
using System.Windows.Controls;

namespace WpfApp2;

public partial class SliderControl: UserControl
{
    public static readonly DependencyProperty CornerRadiusValueProperty = DependencyProperty.Register(
        "CornerRadiusValue", typeof(double), typeof(SliderControl), new PropertyMetadata(default(double)));

    public double CornerRadiusValue
    {
        get { return (double)GetValue(CornerRadiusValueProperty); }
        set { SetValue(CornerRadiusValueProperty, value); }
    }

    public SliderControl()
    {
        InitializeComponent();
    }
}
