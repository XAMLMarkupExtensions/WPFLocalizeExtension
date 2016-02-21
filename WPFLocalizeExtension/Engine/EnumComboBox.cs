#region Copyright information
// <copyright file="EnumComboBox.cs">
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
    using System.Windows.Markup;

    /// <summary>
    /// An extended combobox that is enumerating Enum values.
    /// <para>Use the <see cref="BrowsableAttribute"/> to hide specific entries.</para>
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
            get { return (bool)GetValue(PrependTypeProperty); }
            set { SetValue(PrependTypeProperty, value); }
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
            get { return (string)GetValue(SeparatorProperty); }
            set { SetValue(SeparatorProperty, value); }
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
            get { return (string)GetValue(PrefixProperty); }
            set { SetValue(PrefixProperty, value); }
        }
        #endregion

        #region XamlWriter Hack
        /// <summary>
        /// Overwrite and bypass the Items property.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ItemCollection Items
        {
            get { return base.Items; }
        }

        private bool shouldSerializeTemplate = false;

        protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
        {
            if (oldItemTemplate != null)
                shouldSerializeTemplate = true;

            base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
        }

        protected override bool ShouldSerializeProperty(DependencyProperty dp)
        {
            if ((dp == ItemTemplateProperty) && !shouldSerializeTemplate)
                return false;
            else
                return base.ShouldSerializeProperty(dp);
        } 
        #endregion

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
                
                ItemsSource = items;
            }
            catch
            {
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
