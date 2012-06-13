namespace WPFLocalizeExtension.TypeConverters
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.Windows.Media.Imaging;
    #endregion

    /// <summary>
    /// Register missing type converters here.
    /// Based on <see cref="https://github.com/MrCircuit/WPFLocalizationExtension"/>
    /// </summary>
    public static class RegisterMissingTypeConverters
    {
        private static bool registered = false;

        public static void Register()
        {
            if (registered)
                return;
            
            TypeDescriptor.AddAttributes(typeof(BitmapSource), new Attribute[] { new TypeConverterAttribute(typeof(BitmapSourceTypeConverter)) });

            registered = true;
        }
    }
}
