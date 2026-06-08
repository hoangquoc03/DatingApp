using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DatingApp.Desktop.Converters;

/// <summary>
/// Converter để chuyển đổi chuỗi rỗng/null thành Visibility.Collapsed (hoặc ngược lại nếu dùng tham số "Inverse")
/// </summary>
public class NullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNullOrEmpty = value == null || (value is string str && string.IsNullOrWhiteSpace(str));
        
        bool invert = parameter is string param && param.Equals("Inverse", StringComparison.OrdinalIgnoreCase);

        if (invert)
        {
            return isNullOrEmpty ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
