Beginning with v2.1.3 the extension also features an Enum value localization technique. To achieve this one has to employ the new **LocProxy** class. Just pass the particular enum value to the **Source** property and check the right setting of the **PrependType** flag. If this is used, you may also specify a **Separator** that will be used between the type suffix and the value itself leading to key entries like this "MyEnum_MyValue" using underscore as the separator. This allows us to create unique localization keys for different enum types and their values. To get the localized value, just bind your text element to the **Result** property of the **LocProxy**:
```xaml
<lex:LocProxy Source="{Binding}" x:Name="Proxy" PrependType="True" />
<TextBlock Text="{Binding Result, ElementName=Proxy}" Margin="2" FontWeight="Normal" />
```
In general, this proxy class triggers the _ToString_ function of the object that is bound to the **Source** property. It is therefore also applicable to all other kinds of objects where you need value localization.

To enhance this feature, also an **EnumComboBox** class was introduced with v2.1.3. Just feed your enum type to the **Type** property of this class and it does the rest for you, provided, that you correctly style the entries, e.g. using the XAML code snippet from above. You may also hide particular enum values using the **BrowsableAttribute**. A complete example is included in the _AssemblyTest_ example located in the source code of the library.

***
Previous topic: [Localization providers](Localization-providers)