#region Copyright information
// <copyright file="ThicknessConverter.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension/blob/master/LICENSE
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace WPFLocalizeExtension.TypeConverters
{
    #region Usings
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    #endregion

    /// <summary>
    /// A converter for the type <see cref="Thickness"/>.
    /// </summary>
    public class ThicknessConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var result = new Thickness();
            double d1, d2, d3, d4;

            if (value is string s)
            {
                var parts = s.Split(",".ToCharArray());

                switch (parts.Length)
                {
                    case 1:
                        double.TryParse(parts[0], NumberStyles.Any, culture, out d1);
                        result = new Thickness(d1);
                        break;

                    case 2:
                        double.TryParse(parts[0], NumberStyles.Any, culture, out d1);
                        double.TryParse(parts[1], NumberStyles.Any, culture, out d2);
                        result = new Thickness(d1, d2, d1, d2);
                        break;

                    case 4:
                        double.TryParse(parts[0], NumberStyles.Any, culture, out d1);
                        double.TryParse(parts[1], NumberStyles.Any, culture, out d2);
                        double.TryParse(parts[2], NumberStyles.Any, culture, out d3);
                        double.TryParse(parts[3], NumberStyles.Any, culture, out d4);
                        result = new Thickness(d1, d2, d3, d4);
                        break;
                }
            }

            return result;
        }
    }
}