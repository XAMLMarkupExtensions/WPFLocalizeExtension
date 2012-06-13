namespace SLLocalizeExtension.TypeConverters
{
    using System.ComponentModel;
    using System.Globalization;
    using System;

    public class EnumConverter : TypeConverter
    {
        private Type targetType;

        public EnumConverter(Type targetType)
        {
            this.targetType = targetType;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
                return Enum.Parse(targetType, (string)value, true);
            else
                return null;
        }
    }
}
