The LocalizationExtension is designed for and tested under the following frameworks: 

* WPF with .NET 3.5+

The project comes along with the ability to plugin any custom localization provider that implements the _ILocalizationProvider_ interface. The support of resx of previous versions was transferred to such a provider that also serves as the default provider (can be changed, see [Localization providers](Localization-providers)). The resx files can be distributed over several assemblies in the project. As an example, another custom provider for CSV files was implemented.

***
Previous topic: [MarkupExtension basics](MarkupExtension-basics)  
Next topic: [Change log](Change-log)