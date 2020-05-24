#region Copyright information
// <copyright file="BLoc.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension/blob/master/LICENSE
// </copyright>
// <author>Konrad Mattheis</author>
#endregion

namespace WPFLocalizeExtension.ValueConverters
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Data;
    using System.Windows.Markup;
    using WPFLocalizeExtension.Engine;
    using WPFLocalizeExtension.Extensions;
    #endregion

    /// <summary>
    /// Takes given value as resource key and translates it. If no text is found, the received value is returned.
    /// </summary>
    public class TranslateConverter : MarkupExtension, IValueConverter, IMultiValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                try
                {
                    culture = LocalizeDictionary.Instance.SpecificCulture;

                    return LocExtension.GetLocalizedValue(targetType, value.ToString(), culture, null);
                }
                catch
                { }
            }

            return null;
        }

        /// <inheritdoc/>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 0)
                return this.Convert(values[0], targetType, parameter, culture);

            return null;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new TranslateConverter();
        }
    }
}
