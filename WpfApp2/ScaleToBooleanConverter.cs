using System;
using System.Globalization;
using System.Windows.Data;
using WpfApp2;

namespace WpfApp2;

public class ScaleToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        // 尝试将绑定的值（当前选中的缩放比例）和参数（MenuItem对应的缩放比例）进行比较
        double currentScale = (double)value;
        if (double.TryParse(parameter.ToString(), out double parameterValue))
        {
            return currentScale == parameterValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 当用户点击某个菜单项时，根据是否勾选该项来决定返回哪个缩放比例值
        if ((bool)value)
        {
            if (double.TryParse(parameter.ToString(), out double scale))
            {
                return scale;
            }
        }
        return Binding.DoNothing; // 如果没有勾选任何项或发生错误，则不作任何改变
    }
}