#region Copyright information
// <copyright file="LocExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension/blob/master/LICENSE
// </copyright>
// <author>Konrad Mattheis</author>
#endregion

namespace WPFLocalizeExtension.Deprecated.Extensions
{
    using System.Windows.Markup;

    /// <inheritdoc/>>
    [ContentProperty("ResourceIdentifierKey")]
    public class LocExtension : WPFLocalizeExtension.Extensions.LocBaseExtension
    {
        /// <inheritdoc/>>
        public LocExtension() : base() { }

        /// <inheritdoc/>>
        public LocExtension(object key) : base(key) { }
    }
}
