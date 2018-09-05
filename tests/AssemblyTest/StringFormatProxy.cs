namespace AssemblyTest
{
    using System.Windows;
    using System;
    
    public class StringFormatProxy : FrameworkElement
    {
        /// <summary>
        /// <see cref="DependencyProperty"/> StringFormat.
        /// </summary>
        public static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register(
                "StringFormat",
                typeof(string),
                typeof(StringFormatProxy),
                new PropertyMetadata("{0}", DataChanged));

        /// <summary>
        /// The format of the result string.
        /// </summary>
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        /// <summary>
        /// <see cref="DependencyProperty"/> Value.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(object),
                typeof(StringFormatProxy),
                new PropertyMetadata("{0}", DataChanged));

        /// <summary>
        /// The value of the result string.
        /// </summary>
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void DataChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var sfp = sender as StringFormatProxy;
            if (sfp != null && sfp.StringFormat != null)
                sfp.Result = String.Format(sfp.StringFormat, sfp.Value);
        }

        /// <summary>
        /// <see cref="DependencyProperty"/> Result.
        /// </summary>
        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(
                "Result",
                typeof(string),
                typeof(StringFormatProxy),
                new PropertyMetadata(null));

        /// <summary>
        /// The result string.
        /// </summary>
        public string Result
        {
            get { return (string)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }
    }
}
