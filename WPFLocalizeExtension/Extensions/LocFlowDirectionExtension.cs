using System;
using System.Windows;
using System.Windows.Markup;
using WPFLocalizeExtension.BaseExtensions;
using WPFLocalizeExtension.Engine;

namespace WPFLocalizeExtension.Extensions
{
    /// <summary>
    /// <c>BaseLocalizeExtension</c> for <see cref="FlowDirection"/> values
    /// </summary>
    [MarkupExtensionReturnType(typeof(FlowDirection))]
    public class LocFlowDirectionExtension : BaseLocalizeExtension<FlowDirection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocFlowDirectionExtension"/> class.
        /// </summary>
        public LocFlowDirectionExtension() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocFlowDirectionExtension"/> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public LocFlowDirectionExtension(string key) : base(key) { }

        /// <summary>
        /// Provides the Value for the first Binding as <see cref="LocFlowDirectionExtension"/>
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="System.Windows.Markup.IProvideValueTarget"/> provided from the <see cref="MarkupExtension"/>
        /// </param>
        /// <returns>The founded item from the .resx directory or LeftToRight if not founded</returns>
        /// <exception cref="System.InvalidOperationException">
        /// thrown if <paramref name="serviceProvider"/> is not type of <see cref="System.Windows.Markup.IProvideValueTarget"/>
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// thrown if the founded object is not type of <see cref="FlowDirection"/>
        /// </exception>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object obj = base.ProvideValue(serviceProvider) ?? "LeftToRight";

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
                    "ResourceKey '{0}' returns '{1}' which is not type of FlowDirection",
                    this.Key,
                    obj.GetType().FullName));
        }

        /// <summary>
        /// see <c>BaseLocalizeExtension</c>
        /// </summary>
        protected override void HandleNewValue()
        {
            var obj = LocalizeDictionary.Instance.GetLocalizedObject<object>(this.Assembly, this.Dict, this.Key, this.GetForcedCultureOrDefault());
            this.SetNewValue(this.FormatOutput(obj));
        }

        /// <summary>
        /// This method is used to modify the passed object into the target format
        /// </summary>
        /// <param name="input">The object that will be modified</param>
        /// <returns>Returns the modified object</returns>
        protected override object FormatOutput(object input)
        {
            if (LocalizeDictionary.Instance.GetIsInDesignMode() && this.DesignValue != null)
            {
                try
                {
                    return Enum.Parse(typeof(FlowDirection), (string) this.DesignValue, true);
                }
                catch
                {
                    return null;
                }
            }

            return Enum.Parse(typeof(FlowDirection), (string)input, true);
        }
    }
}