using System;
using System.Globalization;
using System.Windows.Data;

namespace GapTextWpfTest.Converters
{
    public class ObjectTypeEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // returns true if and only if value is of type parameter
            var comparedType = parameter as Type;
            return comparedType != null && comparedType.IsInstanceOfType(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
