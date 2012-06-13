#if SILVERLIGHT
namespace SLLocalizeExtension.TypeConverters
#else
namespace WPFLocalizeExtension.TypeConverters
#endif
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;

    public class ThicknessConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return sourceType == typeof(string);
        }

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

