#region Copyright information
// <copyright file="XmlnsDefinitions.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension/blob/master/LICENSE
// </copyright>
// <author>Uwe Mayer</author>
#endregion

using System.Windows.Markup;

// Register this namespace one with prefix
[assembly: XmlnsDefinition("http://wpflocalizeextension.codeplex.com", "WPFLocalizeExtension.Engine")]
[assembly: XmlnsDefinition("http://wpflocalizeextension.codeplex.com", "WPFLocalizeExtension.Extensions")]
[assembly: XmlnsDefinition("http://wpflocalizeextension.codeplex.com", "WPFLocalizeExtension.Providers")]
[assembly: XmlnsDefinition("http://wpflocalizeextension.codeplex.com", "WPFLocalizeExtension.TypeConverters")]
[assembly: XmlnsDefinition("http://wpflocalizeextension.codeplex.com", "WPFLocalizeExtension.ValueConverters")]

[assembly: XmlnsDefinition("https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension", "WPFLocalizeExtension.Engine")]
[assembly: XmlnsDefinition("https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension", "WPFLocalizeExtension.Extensions")]
[assembly: XmlnsDefinition("https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension", "WPFLocalizeExtension.Providers")]
[assembly: XmlnsDefinition("https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension", "WPFLocalizeExtension.TypeConverters")]
[assembly: XmlnsDefinition("https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension", "WPFLocalizeExtension.ValueConverters")]

// Assign a default namespace prefix for the schema
[assembly: XmlnsPrefix("http://wpflocalizeextension.codeplex.com", "lex")]
