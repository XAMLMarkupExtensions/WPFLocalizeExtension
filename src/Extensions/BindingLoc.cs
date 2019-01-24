using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using WPFLocalizeExtension.Engine;

namespace WPFLocalizeExtension.Extensions
{
    public class BindingLoc : MarkupExtension, IDictionaryEventListener
    {
        private static BindingLoc test;

        private readonly Binding _binding;

        private BindingExpression _bindingExpression;

        private FrameworkElement _target;

        private DependencyProperty _targetProp;

        public BindingLoc(Binding binding)
        {
            test = this;

            _binding = binding;
            _binding.Converter = new TranslateConverter();

            LocalizeDictionary.DictionaryEvent.AddListener(this);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_bindingExpression == null)
            {
                if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget pvt)
                {
                    _target = pvt?.TargetObject as FrameworkElement;
                    _targetProp = pvt.TargetProperty as DependencyProperty;
                }
            }

            return _binding.ProvideValue(serviceProvider);
        }

        public void ResourceChanged(DependencyObject sender, DictionaryEventArgs e)
        {
            if (_bindingExpression == null)
            {
                _bindingExpression = _target.GetBindingExpression(_targetProp);
            }

            _bindingExpression.UpdateTarget();
        }
    }
}