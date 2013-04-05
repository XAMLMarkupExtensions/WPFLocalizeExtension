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
    using System.ComponentModel;
    using System.Windows;
    using WPFLocalizeExtension.Extensions;

    /// <summary>
    /// A proxy class to localize object strings.
    /// </summary>
    public class LocProxy : FrameworkElement
    {
        /// <summary>
        /// Our own <see cref="LocExtension"/> instance.
        /// </summary>
        private LocExtension ext = null;

        #region Source property
        /// <summary>
        /// The source.
        /// </summary>
        public static DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(object), typeof(LocProxy), new PropertyMetadata(PropertiesChanged));

        /// <summary>
        /// The backing property for <see cref="LocProxy.SourceProperty"/>
        /// </summary>
        [Category("Common")]
        public object Source
        {
            get { return (object)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        #endregion

        #region PrependType property
        /// <summary>
        /// This flag determines, if the type should be added using the given separator.
        /// </summary>
        public static DependencyProperty PrependTypeProperty = DependencyProperty.Register("PrependType", typeof(bool), typeof(LocProxy), new PropertyMetadata(false, PropertiesChanged));

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
        public static DependencyProperty SeparatorProperty = DependencyProperty.Register("Separator", typeof(string), typeof(LocProxy), new PropertyMetadata("_", PropertiesChanged));

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

        #region Readonly result property
        /// <summary>
        /// The result.
        /// </summary>
        public static DependencyPropertyKey ResultProperty = DependencyProperty.RegisterReadOnly("Result", typeof(string), typeof(LocProxy), new PropertyMetadata(""));

        /// <summary>
        /// The backing property for <see cref="LocProxy.ResultProperty"/>
        /// </summary>
        [Category("Common")]
        public string Result
        {
            get { return (string)GetValue(ResultProperty.DependencyProperty) ?? this.Source.ToString(); }
            set { SetValue(ResultProperty, value); }
        }
        #endregion

        /// <summary>
        /// A notification handler for the <see cref="LocProxy.SourceProperty"/>.
        /// </summary>
        /// <param name="d">The object.</param>
        /// <param name="e">The event arguments.</param>
        private static void PropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var proxy = d as LocProxy;
            if (proxy != null)
            {
                var source = proxy.Source;
                if (source != null)
                {
                    var key = source.ToString();

                    if (proxy.PrependType)
                        key = source.GetType().Name + proxy.Separator + key;

                    proxy.ext.Key = key;
                }
            }
        }

        /// <summary>
        /// Creates a new enum localizer.
        /// </summary>
        public LocProxy()
        {
            ext = new LocExtension();
            ext.SetBinding(this, GetType().GetProperty("Result"));
        }
    }
}