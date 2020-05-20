Localization is the task to adapt values such as strings, colors or the text flow direction to the specific language and culture of the user. To fulfill this task, two major prerequisites are needed: 

* The designer has to decide **where** to apply a localized value. This means he needs a tool to mark the spot and to refer to a certain key in the translation dictionary. Note: Not all values have to be localized (e.g. usually the width of a control is calculated from the content size in an auto-layout application). 
* The designer and/or a translation agency has to fill out a cross-table containing all the keys mentioned above and values for each culture that should be supported by the application.

This project offers the tool to easily define which value has to be localized to the developer - already in design time. It even tries to find the right key if none is given thus speeding up localized XAML coding while still providing a good code readability.
