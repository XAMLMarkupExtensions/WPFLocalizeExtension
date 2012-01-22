namespace WPFLocalizeExtension.Engine
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;
    using WPFLocalizeExtension.Extensions;
    #endregion

    /// <summary>
    /// A binding proxy class that accepts bindings and forwards them to the LocExtension.
    /// Based on: <see cref="http://www.codeproject.com/Articles/71348/Binding-on-a-Property-which-is-not-a-DependencyPro"/>
    /// </summary>
    public class LocBinding : FrameworkElement
    {
        #region Source DP
        //We don't know what will be the Source/target type so we keep 'object'.
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(LocBinding),
            new FrameworkPropertyMetadata()
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });

        public Object Source
        {
            get { return GetValue(LocBinding.SourceProperty); }
            set { SetValue(LocBinding.SourceProperty, value); }
        }
        #endregion

        #region Target LocExtension
        private LocExtension target = null;
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
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == LocBinding.SourceProperty.Name)
            {
                if (!object.ReferenceEquals(this.Source, target) && (target != null))
                    target.Key = this.Source.ToString();
            }
        }
        #endregion
    }
}
