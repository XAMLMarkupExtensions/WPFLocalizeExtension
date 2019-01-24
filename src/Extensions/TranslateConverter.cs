using System;
using System.Globalization;
using System.Windows.Data;
using WPFLocalizeExtension.Engine;

namespace WPFLocalizeExtension.Extensions
{
    public class TranslateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (LocalizeDictionary.Instance.GetIsInDesignMode())
                return $"Key: {value}";
            else
                return LocExtension.GetLocalizedValue<string>(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}