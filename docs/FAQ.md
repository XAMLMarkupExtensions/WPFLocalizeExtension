# FAQ

## Strong Name and signing

The current nuget is public signed, this is in general a good compromise, but there are sometimes issue like:

* "Could not load file or assembly 'WPFLocalizeExtension' ... Strong name signature could not be verified
    * xUnit can be solve with disable shadowCoping see #225
    * in general integrate [StrongNameSigner](https://github.com/brutaldev/StrongNameSigner)

There is also a long discussion here just some issues: #225 #150 #141 #110 #106


## Issued Design mode

We try to do our best to have the extension as best as possible working in Visual Studio Designer, but in some cases
this doesn't work even everything is fine. Here some points / steps that can help:
1. restart / reopen (xaml, sln, complete Visualt Studio)
2. remove cache (bin/obj dirs + %USERPROFILE%\AppData\Local\Microsoft\VisualStudio\\**[VS VERSION]**\Designer\ShadowCache)
3. check .net compile proj with any cpu or 32bit see #217


## Missing Keys

If you like to see what keys are not translated you can easily register to the following event.

```csharp
LocalizeDictionary.Instance.MissingKeyEvent += Instance_MissingKeyEvent;
```

The issue or bad design is that this is only triggered if you use the LocExtension, BLoc and not if you make codebehind translation.
If you do that you should use a different Event.

```csharp
LocalizeDictionary.Instance.GetLocalizedObject(....) 

LocalizeDictionary.Instance.DefaultProvider.ProviderError  += ...
```

## Binding cannot be changed after it has been used

Binding does not support any changes on Properties after it is bound the firts time. So if you like to do thinks like:

```xaml
{Binding Source={lex:Loc Foo}}
{Binding StringFormat={lex:Loc Bar}}
```
This will crash. For StringFormat use cases we have implemented a [StringFormatConverter](ValueConverters.md).

## Access modifier for resource assemblies
The LocalizationExtension will fail to look up a localized value, if the resource is compiled with its access modifier set to internal.
To set up the resource build tool to use a public access modifier, open the resource file and change the value of the field.

## NeutralResourcesLanguage in Assembly Manifest
The Assembly.cs contains sometimes the line _[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]_
which has side effects on resource lookups.

Please remove it.

## Bad ResX Filename
As long as you are using custom ResX files that are not located under the Properties folder
in your project you **may not call the file Resource**.
It will break the lookup mechanism for the resources.

## Startuptime for Available Cultures Search with Resx Provider
To reduce the full search through all cultures and all resx files if they are available per
culture you can set the searchlist of the culture. _ResxLocalizationProviderBase.SearchCultures_

<details><summary>Example/summary>
<p>
```csharp
 (LocalizeDictionary.Instance.DefaultProvider as ResxLocalizationProvider).SearchCultures =
                new List<System.Globalization.CultureInfo>()
                {
                    System.Globalization.CultureInfo.GetCultureInfo("de-de"),
                    System.Globalization.CultureInfo.GetCultureInfo("en"),
                    System.Globalization.CultureInfo.GetCultureInfo("he"),
                    System.Globalization.CultureInfo.GetCultureInfo("ar"),
                };
```
</p>
</details>