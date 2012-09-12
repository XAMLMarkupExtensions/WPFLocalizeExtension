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
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;    
#if SILVERLIGHT
    using SLLocalizeExtension.Extensions;
#else
    using WPFLocalizeExtension.Extensions;
#endif
    #endregion

    /// <summary>
    /// A binding proxy class that accepts bindings and forwards them to the LocExtension.
    /// Based on: http://www.codeproject.com/Articles/71348/Binding-on-a-Property-which-is-not-a-DependencyPro
    /// </summary>
    public class LocBinding : FrameworkElement
    {
        #region Source DP
        /// <summary>
        /// We don't know what will be the Source/target type so we keep 'object'.
        /// </summary>
#if SILVERLIGHT
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(LocBinding),
            new PropertyMetadata(null, OnPropertyChanged));
#else
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(LocBinding),
            new FrameworkPropertyMetadata(OnPropertyChanged)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });
#endif

        /// <summary>
        /// The source.
        /// </summary>
        public Object Source
        {
            get { return GetValue(LocBinding.SourceProperty); }
            set { SetValue(LocBinding.SourceProperty, value); }
        }
        #endregion

        #region Target LocExtension
        private LocExtension target = null;
        /// <summary>
        /// The target extension.
        /// </summary>
        public LocExtension Target
        {
            get { return target; }
            set
            {
                target = value;
                if ((target != null) && (this.Source != null))
                    target.Key = this.Source.ToString();
            }
        }
        #endregion

        #region OnPropertyChanged
        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var locBinding = obj as LocBinding;

            if (locBinding != null && args.Property == LocBinding.SourceProperty)
            {
                if (!object.ReferenceEquals(locBinding.Source, locBinding.target) && (locBinding.target != null) && (locBinding.Source != null))
                    locBinding.target.Key = locBinding.Source.ToString();
            }
        }
        #endregion
    }
}
