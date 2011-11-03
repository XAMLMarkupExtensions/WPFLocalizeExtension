using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using WPFLocalizeExtension.BaseExtensions;
using WPFLocalizeExtension.Engine;

namespace WPFLocalizeExtension.Extensions
{
    /// <summary>
    /// <c>BaseLocalizeExtension</c> for Thickness values
    /// </summary>
    [MarkupExtensionReturnType(typeof(Thickness))]
    public class LocThicknessExtension : BaseLocalizeExtension<Thickness>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocThicknessExtension"/> class.
        /// </summary>
        public LocThicknessExtension() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocThicknessExtension"/> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public LocThicknessExtension(string key) : base(key) { }

        /// <summary>
        /// Provides the Value for the first Binding as Thickness
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="System.Windows.Markup.IProvideValueTarget"/> provided from the <see cref="MarkupExtension"/>
        /// </param>
        /// <returns>The founded item from the .resx directory or null if not founded</returns>
        /// <exception cref="System.InvalidOperationException">
        /// thrown if <paramref name="serviceProvider"/> is not type of <see cref="System.Windows.Markup.IProvideValueTarget"/>
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// thrown if the founded object is not type of Thickness
        /// </exception>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object obj = base.ProvideValue(serviceProvider);

            if (obj == null)
            {
                return null;
            }

            if (this.IsTypeOf(obj.GetType(), typeof(BaseLocalizeExtension<>)))
            {
                return obj;
            }
            
            if (obj.GetType().Equals(typeof(string)))
            {
                return this.FormatOutput(obj);
            }

            throw new NotSupportedException(
                string.Format(
                    "ResourceKey '{0}' returns '{1}' which is not type of double",
                    this.Key,
                    obj.GetType().FullName));
        }

        /// <summary>
        /// see <c>BaseLocalizeExtension</c>
        /// </summary>
        protected override void HandleNewValue()
        {
            object obj = LocalizeDictionary.Instance.GetLocalizedObject<object>(this.Assembly, this.Dict, this.Key, this.GetForcedCultureOrDefault());
            this.SetNewValue(this.FormatOutput(obj));
        }

        /// <summary>
        /// This method is used to modify the passed object into the target format
        /// </summary>
        /// <param name="input">The object that will be modified</param>
        /// <returns>Returns the modified object</returns>
        protected override object FormatOutput(object input)
        {
            MethodInfo method = typeof(ThicknessConverter).GetMethod("FromString", BindingFlags.Static | BindingFlags.NonPublic);

            if (LocalizeDictionary.Instance.GetIsInDesignMode() && this.DesignValue != null)
            {
                try
                {
                    return
                        (Thickness) method.Invoke(null, new[] { this.DesignValue, new CultureInfo("en-US") });
                }
                catch
                {
                    return null;
                }
            }

            return (Thickness)method.Invoke(null, new[] { input, new CultureInfo("en-US") });
        }
    }
}