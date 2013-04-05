#region Copyright information
// <copyright file="LocBinding.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

#if SILVERLIGHT
namespace SLLocalizeExtension.Engine
#else
namespace WPFLocalizeExtension.Engine
#endif
{
    using System;
    using System.Linq;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Collections.Generic;

    /// <summary>
    /// An extended combobox that is enumerating Enum values.
    /// <para>Use the <see cref="BrowsableAttribute"/> to hide specific entries.</para>
    /// </summary>
    public class EnumComboBox : ComboBox
    {
        /// <summary>
        /// The Type.
        /// </summary>
        public static DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(Type), typeof(EnumComboBox), new PropertyMetadata(TypeChanged));

        /// <summary>
        /// The backing property for <see cref="EnumComboBox.TypeProperty"/>
        /// </summary>
        [Category("Common")]
        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        private static void TypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ecb = d as EnumComboBox;

            if (ecb == null)
                return;

            ecb.SetType(ecb.Type);
        }

        private void SetType(Type type)
        {
            try
            {
                var items = new List<object>();
                var values = Enum.GetValues(type);

                // First we need to get list of all enum fields
                var fields = type.GetFields();

                foreach (var field in fields)
                {
                    // Continue only for normal fields
                    if (field.IsSpecialName)
                        continue;

                    // Get the first BrowsableAttribute and add the item accordingly.
                    var attr = field.GetCustomAttributes(false).OfType<BrowsableAttribute>().FirstOrDefault();

                    if (attr == null || attr.Browsable)
                        items.Add(field.GetValue(0));
                }

                this.ItemsSource = items;
            }
            catch
            {
            }
        }
    }
}
