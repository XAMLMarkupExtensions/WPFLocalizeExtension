#region Copyright information
// <copyright file="ParentChangedNotifierHelper.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
#endregion

#if NET35
namespace WPFLocalizeExtension.Engine
{
    #region Usings
    using System;
    using System.Runtime.Serialization;
    #endregion

    /// <summary>
    /// This class implements an wrapper for .NET35, because this is starting from NET45 of <see cref="WeakReference"/>.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    public class WeakReference<T> : WeakReference
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="target">The target.</param>
		public WeakReference(T target) : base(target)
        {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="trackResurrection">The track resurrection flag.</param>
        public WeakReference(T target, bool trackResurrection)
            : base(target, trackResurrection)
        {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected WeakReference(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public bool TryGetTarget(out T target)
	{
            var baseTarget = base.Target;
            if (baseTarget != null)
            {
                target = (T)baseTarget ;
                return true;
            }
            target = default(T);
            return false;
        }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        //public new T Target
        //{
        //    get
        //    {
        //        var baseTarget = base.Target;
        //        if (IsAlive && baseTarget != null)
        //        {
        //            return (T)base.Target;
        //        }
        //        return default(T);
        //    }
        //    set { base.Target = value; }
        //}
    }
}
#endif
