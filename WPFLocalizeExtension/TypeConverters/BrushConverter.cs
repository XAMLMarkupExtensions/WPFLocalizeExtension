#region Copyright information
// <copyright file="BrushConverter.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

#if WINDOWS_PHONE
namespace WP7LocalizeExtension.TypeConverters
#else
namespace SLLocalizeExtension.TypeConverters
#endif
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Media;
    using System;

    /// <summary>
    /// A converter for the type <see cref="Brush"/>.
    /// </summary>
    public class BrushConverter : TypeConverter
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
            SolidColorBrush result = new SolidColorBrush();

            if (value is string)
            {
                var s = (string)value;

                if (s.StartsWith("#"))
                {
                    byte a = 0, r = 0, g = 0, b = 0;
                    int offset = 1;

                    if (s.Length == 9)
                    {
                        byte.TryParse(s.Substring(offset, 2), out a);
                        offset += 2;
                    }
                    else if (s.Length != 7)
                        return result;

                    byte.TryParse(s.Substring(offset, 2), out r);
                    byte.TryParse(s.Substring(offset + 2, 2), out g);
                    byte.TryParse(s.Substring(offset + 4, 2), out b);

                    var c = new Color() { A = a, B = b, G = g, R = r };
                    result = new SolidColorBrush(c);
                }
                else
                {
#if SILVERLIGHT
                    result = new SolidColorBrush(ColorHelper.FromName(s));
#else
                    foreach (var p in typeof(Colors).GetProperties())
                    {
                        if (p.Name == s)
                            result = new SolidColorBrush((Color)p.GetValue(null, null));
                    }
#endif
                }
            }

            return result;
        }
    }
}

