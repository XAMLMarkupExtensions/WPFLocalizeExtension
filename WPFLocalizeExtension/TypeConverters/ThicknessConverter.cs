#region Copyright information
// <copyright file="ThicknessConverter.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

#if WINDOWS_PHONE
namespace WP7LocalizeExtension.TypeConverters
#elif SILVERLIGHT
namespace SLLocalizeExtension.TypeConverters
#else
namespace WPFLocalizeExtension.TypeConverters
#endif
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;

    /// <summary>
    /// A converter for the type <see cref="Thickness"/>.
    /// </summary>
    public class ThicknessConverter : TypeConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="Type"/> that represents the type you want to convert from.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
        /// <param name="value">The <see cref="Object"/> to convert.</param>
        /// <returns>An <see cref="Object"/> that represents the converted value.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            Thickness result = new Thickness();
            double d1 = 0;
            double d2 = 0;
            double d3 = 0;
            double d4 = 0;

            if (value is string)
            {
                var parts = ((string)value).Split(",".ToCharArray());

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

