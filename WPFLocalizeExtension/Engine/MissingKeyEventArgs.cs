#region Copyright information
// <copyright file="MissingKeyEventArgs.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace WPFLocalizeExtension.Engine
{
    using System;

    /// <summary>
    /// Event arguments for a missing key event.
    /// </summary>
    public class MissingKeyEventArgs : EventArgs
    {
        /// <summary>
        /// The key that is missing or has no data.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// A flag indicating that a reload should be performed.
        /// </summary>
        public bool Reload { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="MissingKeyEventArgs"/>.
        /// </summary>
        /// <param name="key">The missing key.</param>
        public MissingKeyEventArgs(string key)
        {
            this.Key = key;
            this.Reload = false;
        }
    }
}
