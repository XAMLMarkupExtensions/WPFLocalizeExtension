#region Copyright information
// <copyright file="LocBinding.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
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

    public class FullyQualifiedResourceKey
    {
        private readonly String _key;
        public String Key { get { return _key; } }

        private readonly String _assembly;
        public String Assembly { get { return _assembly; } }

        private readonly String _dictionary;
        public String Dictionary { get { return _dictionary; } }

        public FullyQualifiedResourceKey(string key, string assembly = null, string dictionary = null)
        {
          _key = key;
          _assembly = assembly;
          _dictionary = dictionary;
        }

        public override string ToString()
        {
          return String.Join(":", (new[] { Assembly, Dictionary, Key }).Where(x => !String.IsNullOrEmpty(x)).ToArray());
        }

        public static implicit operator string(FullyQualifiedResourceKey fullyQualifiedResourceKey)
        {
          return fullyQualifiedResourceKey.ToString();
        }
  }
}