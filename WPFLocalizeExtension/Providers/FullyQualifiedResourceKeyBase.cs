#region Copyright information
// <copyright file="FullyQualifiedResourceKeyBase.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Justin Pihony</author>
// <author>Uwe Mayer</author>
#endregion

#if WINDOWS_PHONE
namespace WP7LocalizeExtension.Providers
#elif SILVERLIGHT
namespace SLLocalizeExtension.Providers
#else
namespace WPFLocalizeExtension.Providers
#endif
{
    using System;
    using System.Linq;

    /// <summary>
    /// An abstract class for key identification.
    /// </summary>
    public abstract class FullyQualifiedResourceKeyBase
    {
        /// <summary>
        /// Implicit string operator.
        /// </summary>
        /// <param name="fullyQualifiedResourceKey">The object.</param>
        /// <returns>The joined version of the assembly, dictionary and key.</returns>
        public static implicit operator string(FullyQualifiedResourceKeyBase fullyQualifiedResourceKey)
        {
            return fullyQualifiedResourceKey == null ? null : fullyQualifiedResourceKey.ToString();
        }
    }
}