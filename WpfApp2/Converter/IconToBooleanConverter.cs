using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp2;

public class IconToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        Icon iconValue = (Icon)value;
        if (Enum.TryParse<Icon>(parameter.ToString(), out Icon parameterValue))
        {
            return iconValue == parameterValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
        {
            if (Enum.TryParse<Icon>(parameter.ToString(), out Icon icon))
            {
                return icon;
            }
        }
        return Binding.DoNothing;
    }
}