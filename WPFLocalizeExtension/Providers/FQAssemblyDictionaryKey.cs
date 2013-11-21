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
    /// A class that bundles the key, assembly and dictionary information.
    /// </summary>
    public class FQAssemblyDictionaryKey : FullyQualifiedResourceKeyBase
    {
        private readonly String key;
        /// <summary>
        /// The key.
        /// </summary>
        public String Key { get { return key; } }

        private readonly String assembly;
        /// <summary>
        /// The assembly of the dictionary.
        /// </summary>
        public String Assembly { get { return assembly; } }

        private readonly String dictionary;
        /// <summary>
        /// The resource dictionary.
        /// </summary>
        public String Dictionary { get { return dictionary; } }

        /// <summary>
        /// Creates a new instance of <see cref="FullyQualifiedResourceKeyBase"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="assembly">The assembly of the dictionary.</param>
        /// <param name="dictionary">The resource dictionary.</param>
        public FQAssemblyDictionaryKey(string key, string assembly = null, string dictionary = null)
        {
            this.key = key;
            this.assembly = assembly;
            this.dictionary = dictionary;
        }

        /// <summary>
        /// Converts the object to a string.
        /// </summary>
        /// <returns>The joined version of the assembly, dictionary and key.</returns>
        public override string ToString()
        {
            return String.Join(":", (new[] { Assembly, Dictionary, Key }).Where(x => !String.IsNullOrEmpty(x)).ToArray());
        }
    }
}
