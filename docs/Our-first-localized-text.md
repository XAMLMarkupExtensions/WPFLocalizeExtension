With the window or control control properly prepared in the previous section, we can now create our first localized text. Create a key named Test in your resource file(s) and give some distinct values to it (e.g. "Hello World" for culture "en" and "Hallo Welt" for culture "de").
Now, create a button with our extension set to the Content property:
```xaml
<Button Content="{lex:Loc Test}" />
```
Note, that Test was our key in the resource file. That's it - you're done!

### Target types
The LocExtension automatically retrieves the type of the target property and tries to find a _TypeConverter_ for this type. By additionally supplying a converter for _Bitmap_ (from resource files) to _BitmapSource_ (WPF) all normal cases should be covered. If you encounter an unsupported conversion, feel free to write a working converter code and make a pull request to our GIT repository. You may also provide a custom IValueConverter (Converter) along with a converter parameter in the LocExtension.
Enum types are supported in general and do not need any specific converter.
