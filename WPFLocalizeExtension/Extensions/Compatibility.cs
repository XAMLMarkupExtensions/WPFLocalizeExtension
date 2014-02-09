#region Copyright information
// <copyright file="Compatibility.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
// <author>Uwe Mayer</author>
#endregion

#if SILVERLIGHT
namespace SLLocalizeExtension.Extensions
#else
namespace WPFLocalizeExtension.Extensions
#endif
{
#pragma warning disable 1591

    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Markup;
#if SILVERLIGHT
    using SLLocalizeExtension.Engine;
#else
    using WPFLocalizeExtension.Engine;
#endif
    using XAMLMarkupExtensions.Base;
    #endregion

#if SILVERLIGHT
#else
    [MarkupExtensionReturnType(typeof(System.Windows.Media.Brush))]
#endif
    public class LocBrushExtension : LocExtension
    {
        public LocBrushExtension() : base() { }
        public LocBrushExtension(string key) : base(key) { }
    }

#if SILVERLIGHT
#else
    [MarkupExtensionReturnType(typeof(double))]
#endif
    public class LocDoubleExtension : LocExtension
    {
        public LocDoubleExtension() : base() { }
        public LocDoubleExtension(string key) : base(key) { }
    }

#if SILVERLIGHT
#else
    [MarkupExtensionReturnType(typeof(System.Windows.FlowDirection))]
#endif
    public class LocFlowDirectionExtension : LocExtension
    {
        public LocFlowDirectionExtension() : base() { }
        public LocFlowDirectionExtension(string key) : base(key) { }
    }

#if SILVERLIGHT
#else
    [MarkupExtensionReturnType(typeof(System.Windows.Media.Imaging.BitmapSource))]
#endif
    public class LocImageExtension : LocExtension
    {
        public LocImageExtension() : base() { }
        public LocImageExtension(string key) : base(key) { }
    }

#if SILVERLIGHT
#else
    [MarkupExtensionReturnType(typeof(string))]
#endif
    public class LocTextExtension : LocExtension
    {
        #region Constructors
        public LocTextExtension() : base() { }
        public LocTextExtension(string key) : base(key) { }
        #endregion

        #region Enum Definition
        /// <summary>
        /// This enumeration is used to determine the type 
        /// of the return value of <see cref="GetAppendText"/>
        /// </summary>
        protected enum TextAppendType
        {
            /// <summary>
            /// The return value is used as prefix
            /// </summary>
            Prefix,

            /// <summary>
            /// The return value is used as suffix
            /// </summary>
            Suffix
        } 
        #endregion

        #region Variables
        /// <summary>
        /// Holds the local prefix value
        /// </summary>
        private string prefix;

        /// <summary>
        /// Holds the local suffix value
        /// </summary>
        private string suffix;

        /// <summary>
        /// Holds the local format segment array
        /// </summary>
        private string[] formatSegments = new string[5]; 
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a prefix for the localized text
        /// </summary>
        public string Prefix
        {
            get { return this.prefix; }
            set
            {
                this.prefix = value;
            }
        }

        /// <summary>
        /// Gets or sets a suffix for the localized text
        /// </summary>
        public string Suffix
        {
            get { return this.suffix; }
            set
            {
                this.suffix = value;
            }
        }

        /// <summary>
        /// Gets or sets the format segment 1.
        /// This will be used to replace format place holders from the localized text.
        /// <see cref="LocTextLowerExtension"/> and <see cref="LocTextUpperExtension"/> will format this segment.
        /// </summary>
        /// <value>The format segment 1.</value>
        public string FormatSegment1
        {
            get { return this.formatSegments[0]; }
            set
            {
                this.formatSegments[0] = value;
            }
        }

        /// <summary>
        /// Gets or sets the format segment 2.
        /// This will be used to replace format place holders from the localized text.
        /// <see cref="LocTextUpperExtension"/> and <see cref="LocTextLowerExtension"/> will format this segment.
        /// </summary>
        /// <value>The format segment 2.</value>
        public string FormatSegment2
        {
            get { return this.formatSegments[1]; }
            set
            {
                this.formatSegments[1] = value;
            }
        }

        /// <summary>
        /// Gets or sets the format segment 3.
        /// This will be used to replace format place holders from the localized text.
        /// <see cref="LocTextUpperExtension"/> and <see cref="LocTextLowerExtension"/> will format this segment.
        /// </summary>
        /// <value>The format segment 3.</value>
        public string FormatSegment3
        {
            get { return this.formatSegments[2]; }
            set
            {
                this.formatSegments[2] = value;
            }
        }

        /// <summary>
        /// Gets or sets the format segment 4.
        /// This will be used to replace format place holders from the localized text.
        /// <see cref="LocTextUpperExtension"/> and <see cref="LocTextLowerExtension"/> will format this segment.
        /// </summary>
        /// <value>The format segment 4.</value>
        public string FormatSegment4
        {
            get { return this.formatSegments[3]; }
            set
            {
                this.formatSegments[3] = value;
            }
        }

        /// <summary>
        /// Gets or sets the format segment 5.
        /// This will be used to replace format place holders from the localized text.
        /// <see cref="LocTextUpperExtension"/> and <see cref="LocTextLowerExtension"/> will format this segment.
        /// </summary>
        /// <value>The format segment 5.</value>
        public string FormatSegment5
        {
            get { return this.formatSegments[4]; }
            set
            {
                this.formatSegments[4] = value;
            }
        } 
        #endregion

        #region Text Formatting
        /// <summary>
        /// Returns the prefix or suffix text, depending on the supplied <see cref="TextAppendType"/>.
        /// If the prefix or suffix is null, it will be returned a string.empty.
        /// </summary>
        /// <param name="at">The <see cref="TextAppendType"/> defines the format of the return value</param>
        /// <returns>Returns the formated prefix or suffix</returns>
        private string GetAppendText(TextAppendType at)
        {
            // define a return value
            string retVal = string.Empty;

            // check if it should be a prefix, the format will be [PREFIX],
            // or check if it should be a suffix, the format will be [SUFFIX]
            if (at == TextAppendType.Prefix && !string.IsNullOrEmpty(this.prefix))
            {
                retVal = this.prefix ?? string.Empty;
            }
            else if (at == TextAppendType.Suffix && !string.IsNullOrEmpty(this.suffix))
            {
                retVal = this.suffix ?? string.Empty;
            }

            // return the formated prefix or suffix
            return retVal;
        }

        /// <summary>
        /// This method formats the localized text.
        /// If the passed target text is null, string.empty will be returned.
        /// </summary>
        /// <param name="target">The text to format.</param>
        /// <returns>Returns the formated text or string.empty, if the target text was null.</returns>
        protected virtual string FormatText(string target)
        {
            return target ?? string.Empty;
        }

        /// <summary>
        /// This function returns the properly prepared output of the markup extension.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <param name="endPoint">Information about the endpoint.</param>
        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            string textMain = base.FormatOutput(endPoint, info) as String ?? String.Empty;

            try
            {
                // add some format segments, in case that the main text contains format place holders like {0}
                textMain = string.Format(
                    #if SILVERLIGHT
                    LocalizeDictionary.Instance.Culture,
#else
                    LocalizeDictionary.Instance.SpecificCulture,
#endif
                    textMain,
                    this.formatSegments[0] ?? string.Empty,
                    this.formatSegments[1] ?? string.Empty,
                    this.formatSegments[2] ?? string.Empty,
                    this.formatSegments[3] ?? string.Empty,
                    this.formatSegments[4] ?? string.Empty);
            }
            catch (FormatException)
            {
                // if a format exception was thrown, change the text to an error string
                textMain = "TextFormatError: Max 5 Format PlaceHolders! {0} to {4}";
            }

            // get the prefix
            string textPrefix = this.GetAppendText(TextAppendType.Prefix);

            // get the suffix
            string textSuffix = this.GetAppendText(TextAppendType.Suffix);

            // format the text with prefix and suffix to [PREFIX]LocalizedText[SUFFIX]
            textMain = this.FormatText(textPrefix + textMain + textSuffix);

            return textMain;
        } 
        #endregion
    }

#if SILVERLIGHT
#else
    [MarkupExtensionReturnType(typeof(string))]
#endif
    public class LocTextLowerExtension : LocTextExtension
    {
        #region Constructors
        public LocTextLowerExtension() : base() { }
        public LocTextLowerExtension(string key) : base(key) { }
        #endregion

        #region Text Formatting
        /// <summary>
        /// This method formats the localized text.
        /// If the passed target text is null, string.empty will be returned.
        /// </summary>
        /// <param name="target">The text to format.</param>
        /// <returns>
        /// Returns the formated text or string.empty, if the target text was null.
        /// </returns>
        protected override string FormatText(string target)
        {
            return target == null ? string.Empty : target.ToLower(this.GetForcedCultureOrDefault());
        }
        #endregion
    }

#if SILVERLIGHT
#else
    [MarkupExtensionReturnType(typeof(string))]
#endif
    public class LocTextUpperExtension : LocTextExtension
    {
        #region Constructors
        public LocTextUpperExtension() : base() { }
        public LocTextUpperExtension(string key) : base(key) { }
        #endregion

        #region Text Formatting
        /// <summary>
        /// This method formats the localized text.
        /// If the passed target text is null, string.empty will be returned.
        /// </summary>
        /// <param name="target">The text to format.</param>
        /// <returns>
        /// Returns the formated text or string.empty, if the target text was null.
        /// </returns>
        protected override string FormatText(string target)
        {
            return target == null ? string.Empty : target.ToUpper(this.GetForcedCultureOrDefault());
        }
        #endregion
    }

#if SILVERLIGHT
#else
    [MarkupExtensionReturnType(typeof(System.Windows.Thickness))]
#endif
    public class LocThicknessExtension : LocExtension
    {
        public LocThicknessExtension() : base() { }
        public LocThicknessExtension(string key) : base(key) { }
    }

#pragma warning restore 1591
}
