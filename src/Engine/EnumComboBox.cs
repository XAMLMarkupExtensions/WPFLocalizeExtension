#region Copyright information
// <copyright file="EnumComboBox.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Markup;

namespace WPFLocalizeExtension.Engine
{
    /// <summary>
    /// An extended combobox that is enumerating Enum values.
    /// <para>Use the <see cref="T:System.ComponentModel.BrowsableAttribute" /> to hide specific entries.</para>
    /// </summary>
    public class EnumComboBox : ComboBox
    {
        #region Type property
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
            get => (Type)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        private static void TypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is EnumComboBox ecb))
                return;

            ecb.SetType(ecb.Type);
        } 
        #endregion

        #region PrependType property
        /// <summary>
        /// This flag determines, if the type should be added using the given separator.
        /// </summary>
        public static DependencyProperty PrependTypeProperty = DependencyProperty.Register("PrependType", typeof(bool), typeof(EnumComboBox), new PropertyMetadata(false));

        /// <summary>
        /// The backing property for <see cref="LocProxy.PrependTypeProperty"/>
        /// </summary>
        [Category("Common")]
        public bool PrependType
        {
            get => (bool)GetValue(PrependTypeProperty);
            set => SetValue(PrependTypeProperty, value);
        }
        #endregion

        #region Separator property
        /// <summary>
        /// The Separator.
        /// </summary>
        public static DependencyProperty SeparatorProperty = DependencyProperty.Register("Separator", typeof(string), typeof(EnumComboBox), new PropertyMetadata("_"));

        /// <summary>
        /// The backing property for <see cref="LocProxy.SeparatorProperty"/>
        /// </summary>
        [Category("Common")]
        public string Separator
        {
            get => (string)GetValue(SeparatorProperty);
            set => SetValue(SeparatorProperty, value);
        }
        #endregion

        #region Prefix property
        /// <summary>
        /// The Prefix.
        /// </summary>
        public static DependencyProperty PrefixProperty = DependencyProperty.Register("Prefix", typeof(string), typeof(EnumComboBox), new PropertyMetadata(null));

        /// <summary>
        /// The backing property for <see cref="LocProxy.PrefixProperty"/>
        /// </summary>
        [Category("Common")]
        public string Prefix
        {
            get => (string)GetValue(PrefixProperty);
            set => SetValue(PrefixProperty, value);
        }
        #endregion

        #region XamlWriter Hack
        /// <summary>
        /// Overwrite and bypass the Items property.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ItemCollection Items => base.Items;

        private bool _shouldSerializeTemplate;

#pragma warning disable 1591
        protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
        {
            if (oldItemTemplate != null)
                _shouldSerializeTemplate = true;

            base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
        }

        protected override bool ShouldSerializeProperty(DependencyProperty dp)
        {
            if (dp == ItemTemplateProperty && !_shouldSerializeTemplate)
                return false;

            return base.ShouldSerializeProperty(dp);
        }
#pragma warning restore 1591
        #endregion

        private void SetType(Type type)
        {
            try
            {
                var items = new List<object>();

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
                
                ItemsSource = items;
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public EnumComboBox()
        {
            var context = new ParserContext();
            
            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("lex", "http://wpflocalizeextension.codeplex.com");

            var xaml = "<DataTemplate><TextBlock><lex:EnumRun EnumValue=\"{Binding}\"";
            xaml += " PrependType=\"{Binding PrependType, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=lex:EnumComboBox}}\"";
            xaml += " Separator=\"{Binding Separator, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=lex:EnumComboBox}}\"";
            xaml += " Prefix=\"{Binding Prefix, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=lex:EnumComboBox}}\"";
            xaml += " /></TextBlock></DataTemplate>";

            ItemTemplate = (DataTemplate)XamlReader.Parse(xaml, context);
        }
    }
}
