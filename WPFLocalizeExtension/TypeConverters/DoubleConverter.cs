namespace SLLocalizeExtension.TypeConverters
{
    using System.ComponentModel;
    using System.Globalization;

    public class DoubleConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            double result = double.NaN;

            if (value is string)
                double.TryParse((string)value, NumberStyles.Any, culture, out result);

            return result;
        }
    }
}
