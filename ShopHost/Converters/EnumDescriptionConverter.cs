using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace ShopHost.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        var type = value.GetType();
        if (!type.IsEnum) return value.ToString() ?? string.Empty;

        var name = Enum.GetName(type, value);
        if (name == null) return value.ToString() ?? string.Empty;

        var field = type.GetField(name);
        if (field == null) return name;

        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
        {
            return attr.Description;
        }

        return name;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
