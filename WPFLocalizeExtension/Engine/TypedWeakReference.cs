#region Copyright information
// <copyright file="ParentChangedNotifierHelper.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
#endregion

using System;
using System.Runtime.Serialization;

namespace WPFLocalizeExtension.Engine
{
	public class TypedWeakReference<T> : WeakReference
	{
		public TypedWeakReference(T target) : base(target)
		{
		}

		public TypedWeakReference(T target, bool trackResurrection) : base(target, trackResurrection)
		{
		}

		protected TypedWeakReference(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public new T Target
		{
			get { return (T)base.Target; }
			set { base.Target = value; }
		}
	}
}