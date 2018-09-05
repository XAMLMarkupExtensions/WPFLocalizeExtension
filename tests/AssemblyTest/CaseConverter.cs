using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace AssemblyTest
{
    public class CaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string) || !(value is string))
                return Binding.DoNothing;

            var input = (string)value;

            if ((string)parameter == "Upper")
                return input.ToUpper();
            else if ((string)parameter == "Lower")
                return input.ToLower();
            else
                return input;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
