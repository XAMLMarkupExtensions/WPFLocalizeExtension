namespace SLLocalizeExtension.TypeConverters
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Media;

    public class BrushConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return sourceType == typeof(string);
        }

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
                    foreach (var p in typeof(Colors).GetProperties())
                    {
                        if (p.Name == s)
                            result = new SolidColorBrush((Color)p.GetValue(null, null));
                    }
                }
            }

            return result;
        }
    }
}

