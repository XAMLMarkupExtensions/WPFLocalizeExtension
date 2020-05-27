#region Copyright information
// <copyright file="BLoc.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension/blob/master/LICENSE
// </copyright>
// <author>Konrad Mattheis</author>
#endregion

namespace WPFLocalizeExtension.ValueConverters
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Markup; 
    #endregion

    /// <summary>
    /// Baseclass for ValueTypeConvertes which implements easy usage as MarkupExtension
    /// </summary>
    public abstract class TypeValueConverterBase : MarkupExtension
    {
        #region MarkupExtension
        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
        #endregion
    }
}
