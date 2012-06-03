#if SILVERLIGHT
namespace SLLocalizeExtension.Extensions
#else
namespace WPFLocalizeExtension.Extensions
#endif
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Collections.Generic;
    
    public class ParentChangedNotifier : DependencyObject
    {
        #region Parent property
        public static DependencyProperty ParentProperty = DependencyProperty.RegisterAttached("Parent", typeof(FrameworkElement), typeof(ParentChangedNotifier), new PropertyMetadata(ParentChanged));

        public static FrameworkElement GetParent(FrameworkElement element)
        {
            return (FrameworkElement)element.GetValue(ParentProperty);
        }

        public static void SetParent(FrameworkElement element, FrameworkElement value)
        {
            element.SetValue(ParentProperty, value);
        } 
        #endregion

        #region ParentChanged callback
        private static void ParentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var notifier = obj as FrameworkElement;

            if (notifier != null && OnParentChangedList.ContainsKey(notifier))
                foreach (var OnParentChanged in OnParentChangedList[notifier])
                    OnParentChanged();
        } 
        #endregion

        private static Dictionary<FrameworkElement, List<Action>> OnParentChangedList = new Dictionary<FrameworkElement, List<Action>>();

        public ParentChangedNotifier(FrameworkElement element, Action onParentChanged)
        {
            if (onParentChanged != null)
            {
                if (!OnParentChangedList.ContainsKey(element))
                    OnParentChangedList.Add(element, new List<Action>());

                OnParentChangedList[element].Add(onParentChanged);
            }

            Binding b = new Binding("Parent");
            b.RelativeSource = new RelativeSource();
            b.RelativeSource.Mode = RelativeSourceMode.FindAncestor;
            b.RelativeSource.AncestorType = typeof(FrameworkElement);

            BindingOperations.SetBinding(element, ParentProperty, b);
        }
    }
}
