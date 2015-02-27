// -----------------------------------------------------------------------
// <copyright file="MultiplyConverter.cs" company="APD Communications">
//     © 2013 APD Communications Limited.
// </copyright>
// -----------------------------------------------------------------------
namespace MemoryTest.Converters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Data;

	/// <summary>
	/// Multiply converter for multiplying the values of multi bindings for expanding lists
	/// </summary>
	public class MultiplyConverter : IMultiValueConverter
	{
		/// <summary>
		/// Converts the specified values.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <param name="targetType">Type of the target.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="culture">The culture.</param>
		/// <returns>the result </returns>
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			/* --- if using to animate height --- */

			// value[0] actual height of items control
			// value[1] tag value

			/* --- if using to animate height --- */

			double result = 1.0;
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i] is double)
				{
					result *= (double)values[i];
				}
				else
				{
					return DependencyProperty.UnsetValue;
				}
			}

			if (parameter != null)
			{
				if (parameter is double)
				{
					result *= (double)parameter;
				}
			}

			return result;
		}

		/// <summary>
		/// Converts a binding target value to the source binding values.
		/// </summary>
		/// <param name="value">The value that the binding target produces.</param>
		/// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// An array of values that have been converted from the target value back to the source values.
		/// </returns>
		/// <exception cref="System.NotImplementedException">Not implemented</exception>
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
