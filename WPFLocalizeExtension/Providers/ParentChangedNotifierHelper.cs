#region Copyright information
// <copyright file="ParentChangedNotifierHelper.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

#if SILVERLIGHT
namespace SLLocalizeExtension.Providers
#else
namespace WPFLocalizeExtension.Providers
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using Engine;
    using XAMLMarkupExtensions.Base;

    /// <summary>
    /// Extension methods for <see cref="DependencyObject"/> in conjunction with the <see cref="ParentChangedNotifier"/>.
    /// </summary>
    public static class ParentChangedNotifierHelper
    {
        /// <summary>
        /// Tries to get a value that is stored somewhere in the visual tree above this <see cref="DependencyObject"/>.
        /// <para>If this is not available, it will register a <see cref="ParentChangedNotifier"/> on the last element.</para>
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject"/>.</param>
        /// <param name="GetFunction">The function that gets the value from a <see cref="DependencyObject"/>.</param>
        /// <param name="ParentChangedAction">The notification action on the change event of the Parent property.</param>
        /// <param name="parentNotifiers">A dictionary of already registered notifiers.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValueOrRegisterParentNotifier<T>(this DependencyObject target, Func<DependencyObject, T> GetFunction, Action<DependencyObject> ParentChangedAction, Dictionary<DependencyObject, ParentChangedNotifier> parentNotifiers)
        {
            var ret = default(T);

            if (target != null)
            {
                var depObj = target;

                while (ret == null)
                {
                    // Try to get the value using the provided GetFunction.
                    ret = GetFunction(depObj);

                    if (ret != null && parentNotifiers.ContainsKey(target))
                    {
                        var notifier = parentNotifiers[target];
                        notifier.Dispose();
                        parentNotifiers.Remove(target);
                    }

                    // Try to get the parent using the visual tree helper. This may fail on some occations.
#if !SILVERLIGHT
                    if (!(depObj is Visual) && !(depObj is Visual3D) && !(depObj is FrameworkContentElement))
                        break;

                    if (depObj is Window)
                        break;
#endif
                    DependencyObject depObjParent = null;
          
#if !SILVERLIGHT
                    if (depObj is FrameworkContentElement)
                        depObjParent = ((FrameworkContentElement)depObj).Parent;
                    else
                    {
                        try { depObjParent = depObj.GetParent(false); }
                        catch { depObjParent = null; }
                    }
#endif

                    if (depObjParent == null)
                    {
                        try { depObjParent = depObj.GetParent(true); }
                        catch { break; }
                    }
                    
                    // If this failed, try again using the Parent property (sometimes this is not covered by the VisualTreeHelper class :-P.
                    if (depObjParent == null && depObj is FrameworkElement)
                        depObjParent = ((FrameworkElement)depObj).Parent;

                    if (ret == null && depObjParent == null)
                    {
                        // Try to establish a notification on changes of the Parent property of dp.
                        if (depObj is FrameworkElement && !parentNotifiers.ContainsKey(target))
                        {
                            var pcn = new ParentChangedNotifier((FrameworkElement)depObj, () =>
                            {
                                // Call the action...
                                ParentChangedAction(target);
                                // ...and remove the notifier - it will probably not be used again.
                                if (parentNotifiers.ContainsKey(target))
                                {
                                    var notifier = parentNotifiers[target];
                                    notifier.Dispose();
                                    parentNotifiers.Remove(target);
                                }
                            });

                            parentNotifiers.Add(target, pcn);
                        }
                        break;
                    }

                    // Assign the parent to the current DependencyObject and start the next iteration.
                    depObj = depObjParent;
                }
            }

            return ret;
        }

        /// <summary>
        /// Tries to get a value that is stored somewhere in the visual tree above this <see cref="DependencyObject"/>.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject"/>.</param>
        /// <param name="GetFunction">The function that gets the value from a <see cref="DependencyObject"/>.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValue<T>(this DependencyObject target, Func<DependencyObject, T> GetFunction)
        {
            var ret = default(T);

            if (target != null)
            {
                var depObj = target;

                while (ret == null)
                {
                    // Try to get the value using the provided GetFunction.
                    ret = GetFunction(depObj);

                    // Try to get the parent using the visual tree helper. This may fail on some occations.
#if !SILVERLIGHT
                    if (!(depObj is Visual) && !(depObj is Visual3D) && !(depObj is FrameworkContentElement))
                        break;
#endif
                    DependencyObject depObjParent = null;

#if !SILVERLIGHT
                    if (depObj is FrameworkContentElement)
                        depObjParent = ((FrameworkContentElement)depObj).Parent;
                    else
#endif
                    {
                        try { depObjParent = depObj.GetParent(true); }
                        catch { break; }
                    }
                    // If this failed, try again using the Parent property (sometimes this is not covered by the VisualTreeHelper class :-P.
                    if (depObjParent == null && depObj is FrameworkElement)
                        depObjParent = ((FrameworkElement)depObj).Parent;

                    if (ret == null && depObjParent == null)
                        break;

                    // Assign the parent to the current DependencyObject and start the next iteration.
                    depObj = depObjParent;
                }
            }

            return ret;
        }

        /// <summary>
        /// Tries to get a value from a <see cref="DependencyProperty"/> that is stored somewhere in the visual tree above this <see cref="DependencyObject"/>.
        /// If this is not available, it will register a <see cref="ParentChangedNotifier"/> on the last element.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject"/>.</param>
        /// <param name="property">A <see cref="DependencyProperty"/> that will be read out.</param>
        /// <param name="ParentChangedAction">The notification action on the change event of the Parent property.</param>
        /// <param name="parentNotifiers">A dictionary of already registered notifiers.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValueOrRegisterParentNotifier<T>(this DependencyObject target, DependencyProperty property, Action<DependencyObject> ParentChangedAction, Dictionary<DependencyObject, ParentChangedNotifier> parentNotifiers)
        {
            return target.GetValueOrRegisterParentNotifier<T>((depObj) =>
            {
                return depObj.GetValueSync<T>(property);
            }, ParentChangedAction, parentNotifiers);
        }

        /// <summary>
        /// Gets the parent in the visual or logical tree.
        /// </summary>
        /// <param name="depObj">The dependency object.</param>
        /// <param name="isVisualTree">True for visual tree, false for logical tree.</param>
        /// <returns>The parent, if available.</returns>
        public static DependencyObject GetParent(this DependencyObject depObj, bool isVisualTree)
        {
#if SILVERLIGHT
            return GetParentInternal(depObj, isVisualTree);
#else
            if (depObj.CheckAccess())
                return GetParentInternal(depObj, isVisualTree);
            else
                return (DependencyObject)depObj.Dispatcher.Invoke(new Func<DependencyObject>(() => GetParentInternal(depObj, isVisualTree)));
#endif
        }

        private static DependencyObject GetParentInternal(DependencyObject depObj, bool isVisualTree)
        {
            if (isVisualTree)
                return VisualTreeHelper.GetParent(depObj);
#if !SILVERLIGHT
            else
                return LogicalTreeHelper.GetParent(depObj);
#else
            else
                return null;
#endif
        }
    }
}
