#region Copyright information
// <copyright file="LocBinding.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Justin Pihony</author>
// <author>Uwe Mayer</author>
#endregion

#if WINDOWS_PHONE
namespace WP7LocalizeExtension.Engine
#elif SILVERLIGHT
namespace SLLocalizeExtension.Engine
#else
namespace WPFLocalizeExtension.Engine
#endif
{
    using System;
    using System.Linq;

    /// <summary>
    /// A class that bundles the key, assembly and dictionary information.
    /// </summary>
    public class FullyQualifiedResourceKey
    {
        private readonly String _key;
        /// <summary>
        /// The key.
        /// </summary>
        public String Key { get { return _key; } }

        private readonly String _assembly;
        /// <summary>
        /// The assembly of the dictionary.
        /// </summary>
        public String Assembly { get { return _assembly; } }

        private readonly String _dictionary;
        /// <summary>
        /// The resource dictionary.
        /// </summary>
        public String Dictionary { get { return _dictionary; } }

        /// <summary>
        /// Creates a new instance of <see cref="FullyQualifiedResourceKey"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="assembly">The assembly of the dictionary.</param>
        /// <param name="dictionary">The resource dictionary.</param>
        public FullyQualifiedResourceKey(string key, string assembly = null, string dictionary = null)
        {
            _key = key;
            _assembly = assembly;
            _dictionary = dictionary;
        }

        /// <summary>
        /// Converts the object to a string.
        /// </summary>
        /// <returns>The joined version of the assembly, dictionary and key.</returns>
        public override string ToString()
        {
            return String.Join(":", (new[] { Assembly, Dictionary, Key }).Where(x => !String.IsNullOrEmpty(x)).ToArray());
        }

        /// <summary>
        /// Implicit string operator.
        /// </summary>
        /// <param name="fullyQualifiedResourceKey">The object.</param>
        /// <returns>The joined version of the assembly, dictionary and key.</returns>
        public static implicit operator string(FullyQualifiedResourceKey fullyQualifiedResourceKey)
        {
            return fullyQualifiedResourceKey == null ? null : fullyQualifiedResourceKey.ToString();
        }
    }
}