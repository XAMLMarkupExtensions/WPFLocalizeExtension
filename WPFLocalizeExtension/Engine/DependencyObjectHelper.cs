#region Copyright information
// <copyright file="DependencyObjectHelper.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

#if WINDOWS_PHONE
namespace WP7LocalizeExtension.Engine
#endif
{
    using System;
    using System.Windows;

    /// <summary>
    /// Extension methods for dependency objects.
    /// </summary>
    public static class DependencyObjectHelper
    {
        /// <summary>
        /// Gets the value thread-safe.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="property">The property.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The value.</returns>
        public static T GetValueSync<T>(this DependencyObject obj, DependencyProperty property)
        {
#if SILVERLIGHT
            return (T)obj.GetValue(property);
#else
            if (obj.CheckAccess())
                return (T)obj.GetValue(property);
            else
                return (T)obj.Dispatcher.Invoke(new Func<object>(() => obj.GetValue(property)));
#endif
        }

        /// <summary>
        /// Sets the value thread-safe.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        public static void SetValueSync<T>(this DependencyObject obj, DependencyProperty property, T value)
        {
#if SILVERLIGHT
            obj.SetValue(property, value);
#else
            if (obj.CheckAccess())
                obj.SetValue(property, value);
            else
                obj.Dispatcher.Invoke(new Action(() => obj.SetValue(property, value)));
#endif
        }
    }
}
