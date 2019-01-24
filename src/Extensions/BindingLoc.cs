using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using WPFLocalizeExtension.Engine;

namespace WPFLocalizeExtension.Extensions
{
    public class BindingLoc : Binding, IDictionaryEventListener
    {
        private static BindingLoc test;

        private BindingExpression _bindingExpression;

        private FrameworkElement _target;

        private DependencyProperty _targetProp;

        public BindingLoc(Binding subBinding)
        {
            test = this;

            Path = subBinding.Path;
            Converter = new TranslateConverter();

            LocalizeDictionary.DictionaryEvent.AddListener(this);
        }

        public object ProvideValue2(IServiceProvider serviceProvider)
        {
            if (_bindingExpression == null)
            {
                if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget pvt)
                {
                    _target = pvt?.TargetObject as FrameworkElement;

                    // if we are inside a template, WPF will call us again when it is applied
                    if (_target == null)
                        return this;

                    _targetProp = pvt.TargetProperty as DependencyProperty;
                }
            }

            return null;// _binding.ProvideValue(serviceProvider);
        }

        public void ResourceChanged(DependencyObject sender, DictionaryEventArgs e)
        {
            if (_bindingExpression == null
                && _target != null
                && _targetProp != null)
            {
                _bindingExpression = _target.GetBindingExpression(_targetProp);
            }

            _bindingExpression?.UpdateTarget();
        }
    }
}