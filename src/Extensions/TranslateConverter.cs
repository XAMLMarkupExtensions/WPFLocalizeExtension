using System;
using System.Globalization;
using System.Windows.Data;

namespace WPFLocalizeExtension.Extensions
{
    public class TranslateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            return LocExtension.GetLocalizedValue<string>(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}