using System;
using System.Globalization;
using System.Windows.Data;
using WPFLocalizeExtension.Engine;

namespace WPFLocalizeExtension.Extensions
{
    /// <summary>
    /// Takes given value as resource key and translates it. If no text is found, the received value is returned.
    /// </summary>
    public class TranslateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (LocalizeDictionary.Instance.GetIsInDesignMode())
                return $"Key: {value}";
            else
                return LocExtension.GetLocalizedValue<string>(value.ToString()) ?? value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}