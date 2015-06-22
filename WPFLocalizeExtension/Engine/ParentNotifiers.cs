#region Copyright information
// <copyright file="ParentChangedNotifierHelper.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using XAMLMarkupExtensions.Base;

namespace WPFLocalizeExtension.Engine
{
	public class ParentNotifiers
	{
		readonly Dictionary<TypedWeakReference<DependencyObject>, TypedWeakReference<ParentChangedNotifier>> _inner = 
			new Dictionary<TypedWeakReference<DependencyObject>, TypedWeakReference<ParentChangedNotifier>>();

		// Dictionary<DependencyObject, ParentChangedNotifier>
		public bool ContainsKey(DependencyObject target)
		{
			return _inner.Keys.Any(x => x.IsAlive && ReferenceEquals(x.Target, target));
		}

		public void Remove(DependencyObject target)
		{
			TypedWeakReference<DependencyObject> singleOrDefault = 
				_inner.Keys.SingleOrDefault(x => ReferenceEquals(x.Target, target));

			if (singleOrDefault != null)
			{
				_inner[singleOrDefault].Target.Dispose();
				_inner.Remove(singleOrDefault);
			}
		}

		public void Add(DependencyObject target, ParentChangedNotifier parentChangedNotifier)
		{
			_inner.Add(new TypedWeakReference<DependencyObject>(target), new TypedWeakReference<ParentChangedNotifier>(parentChangedNotifier));
		}
	}
}