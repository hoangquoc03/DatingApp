using System;
using System.Globalization;
using System.Windows.Data;

namespace DatingApp.Desktop.Converters;

public class ToggleStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? "Khóa" : "Mở khóa";
        }
        return "N/A";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
