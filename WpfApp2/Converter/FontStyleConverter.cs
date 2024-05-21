using System;
using System.Globalization;
using System.Windows.Data;
using WpfApp2;

namespace WpfApp2;

public class FontStyleToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        // 尝试将绑定的值（当前选中的字体样式）和参数（MenuItem对应的字体样式）进行比较
        FontStyleType currentFontStyle = (FontStyleType)value;
        if (Enum.TryParse<FontStyleType>(parameter.ToString(), out FontStyleType parameterValue))
        {
            return currentFontStyle == parameterValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 当用户点击某个菜单项时，根据是否勾选该项来决定返回哪个字体样式枚举
        if ((bool)value)
        {
            if (Enum.TryParse<FontStyleType>(parameter.ToString(), out FontStyleType fontStyle))
            {
                return fontStyle;
            }
        }
        return Binding.DoNothing; // 如果没有勾选任何项或发生错误，则不作任何改变
    }
}