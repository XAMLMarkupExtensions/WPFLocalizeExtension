# Localize Resources

The main Extension that will be used is `{lex:Loc}` and is a Markupextension with a wide range usage.
For some use cases there it is not possible to use a Markupextension in XAML like Multibindings, Trigger, ...
there are substitutes that try to fullfill the job, but they don't have 100% compatibility.

```xaml
{lex:Loc Key=, ForceCulture=, Converter=, ConverterParameter=,}
```

| Parameter          | Description |
|--------------------|-------------|
| Key (default)      | the key for the resource for the resource lookup     |
| ForceCulture       | enforce a different culture for this resource lookup |
| Converter          | like Converter of Binding Class [see](https://docs.microsoft.com/de-de/dotnet/api/system.windows.data.binding.converter) |
| ConverterParameter | like ConverterParameter of Binding Class [see](https://docs.microsoft.com/de-de/dotnet/api/system.windows.data.binding.converter) |

## Key strategy

To make it for you as easy as possible and follow the "Don’t Repeat Yourself" (DRY) principle the Extension try find always the right key even if you just write `{lex:Loc}`
So you can find for example the following line in XAML:
```xaml
<Button x:Name="MyButton" Content="{lex:Loc}" ToolTip="{lex:Loc}" FontSize="{lex:Loc}"/>
```
But how does the magic work?

For a detailed explanation please red the [Key strategy](Keys.md) 

In the this example the Extension would look into the defaultassembly and then into the defaultressource and search for this 3 keys

| Content=...   | MyButton_Content  | string
| ToolTip=...   | MyButton_ToolTip  | string
| FontSize=...  | MyButton_FontSize | double 

If the resource has a different type the extensions try to convert is. This is more details describe in Target types.

## Target types
The LocExtension automatically retrieves the type of the target property and tries to find a _TypeConverter_ for this type. By additionally supplying a converter for _Bitmap_ (from resource files) to _BitmapSource_ (WPF) all normal cases should be covered. 
If you encounter an unsupported conversion, you have different options:
* provide a custom IValueConverter (Converter) along with a converter parameter
* feel free to write a working converter code and make a pull request to our GIT repository.

Enum types are supported in general and do not need any specific converter.

## Binding support for Enum / MVVM localization

If you have an enum or a MVVM use case there you need to have the key in the data model and like to use this as a key for the required resource
the extensions support to add the Binding as Constructor. Because of WPF restriction it is not possible to write it attach it to the key parameter (~~Key={Binding Path=Foo}~~). You have to add it as constructor without a property.

```xaml
{lex:Loc {Binding Path=Foo}}
```

This gives you the full flexibility because the Markupextensions is the pure Binding and this supports Converters, StringFormat, ...

### Enums

If you have now the following situation.

```csharp
public enum TestEnum
{
    Test1,
    Test2
}

...

public TestEnum Foo {get; set;}

... 

Foo = TestEnum.Test1;
```

and you bind this in the xaml with `{lex:Loc {Binding Path=Foo}}` the system will look to the key *Test1*
in the resources. This is okay but in the most cases you like to have a key like *TestEnum.Test1*.
RESX don't support key with dots so the common approach is to replace thge dot with an underscore to the
key *TestEnum_Test1*. To make this now working you have two options.

1. use `{lex:Loc {Binding Path=Foo, StringFormat=TestEnum_{0}}}` with the disadvantage that you have to manually type the TypeName of the enum and that refactoring is not working. On the otherhand you are fully free to change the key.
2. use `{lex:Loc {Binding Path=Foo, Converter={lex:PrependTypeConverter}}}` and get the automated prepending of the type to the key retreval process. This works also for any kind ot .net type, but we see at the moment only the use case for enums. For a details and the possibility to change the separation character read the details [ValueConverters](ValueConverters.md) doc.