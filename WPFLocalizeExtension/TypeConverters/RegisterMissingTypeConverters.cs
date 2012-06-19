#region Copyright information
// <copyright file="RegisterMissingTypeConverters.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

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
    /// </summary>
    public static class RegisterMissingTypeConverters
    {
        /// <summary>
        /// A flag indication if the registration was successful.
        /// </summary>
        private static bool registered = false;
        
        /// <summary>
        /// Registers the missing type converters.
        /// </summary>
        public static void Register()
        {
            if (registered)
                return;
            
            TypeDescriptor.AddAttributes(typeof(BitmapSource), new Attribute[] { new TypeConverterAttribute(typeof(BitmapSourceTypeConverter)) });

            registered = true;
        }
    }
}
