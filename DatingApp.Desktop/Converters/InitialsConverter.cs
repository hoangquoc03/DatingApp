using System;
using System.Globalization;
using System.Windows.Data;

namespace DatingApp.Desktop.Converters;

/// <summary>
/// Lấy chữ viết tắt từ tên (vd: "Nguyễn Văn A" → "NA", "Admin" → "A")
/// Dùng làm fallback avatar khi không có ảnh
/// </summary>
public class InitialsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string name && !string.IsNullOrWhiteSpace(name))
        {
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            return parts[0][0].ToString().ToUpper();
        }
        return "?";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
