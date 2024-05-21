using System;
using System.Globalization;
using System.Windows.Data;
using HandyControl.Themes;

namespace WpfApp2;

public class ThemeToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        Theme themeValue = (Theme)value;
        if (Enum.TryParse<Theme>(parameter.ToString(), out Theme parameterValue))
        {
            return themeValue == parameterValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
        {
            if (Enum.TryParse<Theme>(parameter.ToString(), out Theme theme))
            {
                return theme;
            }
        }
        return Binding.DoNothing;
    }
}
