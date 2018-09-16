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
    /// <summary>
    /// A types version of <see cref="WeakReference"/>.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
	public class TypedWeakReference<T> : WeakReference
	{
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="target">The target.</param>
		public TypedWeakReference(T target) : base(target)
		{
		}

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="trackResurrection">The track resurrection flag.</param>
        public TypedWeakReference(T target, bool trackResurrection)
            : base(target, trackResurrection)
		{
		}

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected TypedWeakReference(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        public new T Target
        {
            get
            {
                var baseTarget = base.Target;
                if (IsAlive && baseTarget != null)
                {
                    return (T)base.Target;
                }
                return default(T);
            }
            set { base.Target = value; }
        }
	}
}