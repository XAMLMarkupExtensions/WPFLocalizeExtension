using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using WPFLocalizeExtension.Engine;

namespace WPFLocalizeExtension.Extensions
{
    /// <summary>
    /// Applies the given <see cref="Binding"/> to the related <see cref="DependencyProperty"/>.
    /// </summary>
    /// <remarks><see cref="MarkupExtension"/>s are not usable within a WPF style.</remarks>
    public class BindingLoc : MarkupExtension, IDictionaryEventListener
    {
        private readonly Binding _binding;

        private BindingExpression _bindingExpression;

        private FrameworkElement _target;

        private DependencyProperty _targetProp;

        public BindingLoc(Binding binding)
        {
            _binding = binding;
            _binding.Converter = new TranslateConverter();

            // Stops garbage collection of this instance in order to receive events from LocalizeDictionary
            // Once, the binding is gone, this instance should be garbage collected.
            _binding.ConverterParameter = this;

            // do not need to remove listener, for LocalizeDictionary keeps weak references
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