# Gap Text

Use a multibinding with the [StringFormatConverter](ValueConverters.md).
The first **parameter** is the StringFormat
All others are parameter.
```XML
<element.Text>
    <MultiBinding Converter={lex:StringFormatConverter}>
        <lex:BLoc Path="assembly:resource:key" />
         <Binding Foo />
         <Binding Bar />
         <Binding ... />
    </MultiBinding>
</element.Text>
```

integrate [SmartFormat](https://github.com/axuno/SmartFormat/wiki) so that we have even a possibility to have real i18n with pluralization,...