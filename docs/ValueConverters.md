# Value Converters

The library delivers some very usefull ValueConverts. All Valueconverters can be used with the standard syntax and the definition as resource, but the also implement a MarkupExtension declaration so that you can write as an example for the TranslateConverter:

```xaml
<Element Converter="{lex:TranslateConverter}" />
```

## TranslateConverter

This converters allows to make the translation process as a converter. The input of the converter is the key for the lookup and the result the translated result.

## PrependTypeConverter

This converter is especially for translation of enums. If you bind to an enum, only the
the Enum Value is used as key for the resource lookup. A commonn approach in the resx
file is to write like:
    MyEnum_Value1 -> Translation for Value1
    MyEnum_Value2 -> Translation for Value2

For a more elegant use this converter automatically prepend the Type of the parameter
to the key.

## StringFormatConverter

This converter solves the issue that Binding unfortunately don't support changing of the StringFormat after the firts usage of the Binding.
The StringFormatConverter expects as the first value the StringFormat and all further Values are given the StringFormat as paramter.
If [smartFormat](https://github.com/axuno/SmartFormat) is available it will be automatically used for the StringFormat for extended possibilities especially pluralisation.