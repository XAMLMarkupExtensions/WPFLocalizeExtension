#region Copyright information
// <copyright file="Compatibility.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
// <author>Uwe Mayer</author>
#endregion

using System;
using System.Windows.Markup;

using WPFLocalizeExtension.Engine;
using XAMLMarkupExtensions.Base;

namespace WPFLocalizeExtension.Extensions
{
#pragma warning disable 1591

    [MarkupExtensionReturnType(typeof(System.Windows.Media.Brush))]
    public class LocBrushExtension : LocExtension
    {
        public LocBrushExtension()
        { }
        public LocBrushExtension(string key) : base(key) { }
    }

    [MarkupExtensionReturnType(typeof(double))]
    public class LocDoubleExtension : LocExtension
    {
        public LocDoubleExtension()
        { }
        public LocDoubleExtension(string key) : base(key) { }
    }

    [MarkupExtensionReturnType(typeof(System.Windows.FlowDirection))]
    public class LocFlowDirectionExtension : LocExtension
    {
        public LocFlowDirectionExtension()
        { }
        public LocFlowDirectionExtension(string key) : base(key) { }
    }

    [MarkupExtensionReturnType(typeof(System.Windows.Media.Imaging.BitmapSource))]
    public class LocImageExtension : LocExtension
    {
        public LocImageExtension()
        { }
        public LocImageExtension(string key) : base(key) { }
    }

    [MarkupExtensionReturnType(typeof(string))]
    public class LocTextExtension : LocExtension
    {
        #region Constructors
        public LocTextExtension()
        { }
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
        private string _prefix;

        /// <summary>
        /// Holds the local suffix value
        /// </summary>
        private string _suffix;

        /// <summary>
        /// Holds the local format segment array
        /// </summary>
        private readonly string[] _formatSegments = new string[5]; 
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a prefix for the localized text
        /// </summary>
        public string Prefix
        {
            get => _prefix;
            set => _prefix = value;
        }

        /// <summary>
        /// Gets or sets a suffix for the localized text
        /// </summary>
        public string Suffix
        {
            get => _suffix;
            set => _suffix = value;
        }

        /// <summary>
        /// Gets or sets the format segment 1.
        /// This will be used to replace format place holders from the localized text.
        /// <see cref="LocTextLowerExtension"/> and <see cref="LocTextUpperExtension"/> will format this segment.
        /// </summary>
        /// <value>The format segment 1.</value>
        public string FormatSegment1
        {
            get => _formatSegments[0];
            set => _formatSegments[0] = value;
        }

        /// <summary>
        /// Gets or sets the format segment 2.
        /// This will be used to replace format place holders from the localized text.
        /// <see cref="LocTextUpperExtension"/> and <see cref="LocTextLowerExtension"/> will format this segment.
        /// </summary>
        /// <value>The format segment 2.</value>
        public string FormatSegment2
        {
            get => _formatSegments[1];
            set => _formatSegments[1] = value;
        }

        /// <summary>
        /// Gets or sets the format segment 3.
        /// This will be used to replace format place holders from the localized text.
        /// <see cref="LocTextUpperExtension"/> and <see cref="LocTextLowerExtension"/> will format this segment.
        /// </summary>
        /// <value>The format segment 3.</value>
        public string FormatSegment3
        {
            get => _formatSegments[2];
            set => _formatSegments[2] = value;
        }

        /// <summary>
        /// Gets or sets the format segment 4.
        /// This will be used to replace format place holders from the localized text.
        /// <see cref="LocTextUpperExtension"/> and <see cref="LocTextLowerExtension"/> will format this segment.
        /// </summary>
        /// <value>The format segment 4.</value>
        public string FormatSegment4
        {
            get => _formatSegments[3];
            set => _formatSegments[3] = value;
        }

        /// <summary>
        /// Gets or sets the format segment 5.
        /// This will be used to replace format place holders from the localized text.
        /// <see cref="LocTextUpperExtension"/> and <see cref="LocTextLowerExtension"/> will format this segment.
        /// </summary>
        /// <value>The format segment 5.</value>
        public string FormatSegment5
        {
            get => _formatSegments[4];
            set => _formatSegments[4] = value;
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
            var retVal = string.Empty;

            // check if it should be a prefix, the format will be [PREFIX],
            // or check if it should be a suffix, the format will be [SUFFIX]
            if (at == TextAppendType.Prefix && !string.IsNullOrEmpty(_prefix))
            {
                retVal = _prefix ?? string.Empty;
            }
            else if (at == TextAppendType.Suffix && !string.IsNullOrEmpty(_suffix))
            {
                retVal = _suffix ?? string.Empty;
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
            var textMain = base.FormatOutput(endPoint, info) as string ?? string.Empty;

            try
            {
                // add some format segments, in case that the main text contains format place holders like {0}
                textMain = string.Format(
                    LocalizeDictionary.Instance.SpecificCulture,
                    textMain,
                    _formatSegments[0] ?? string.Empty,
                    _formatSegments[1] ?? string.Empty,
                    _formatSegments[2] ?? string.Empty,
                    _formatSegments[3] ?? string.Empty,
                    _formatSegments[4] ?? string.Empty);
            }
            catch (FormatException)
            {
                // if a format exception was thrown, change the text to an error string
                textMain = "TextFormatError: Max 5 Format PlaceHolders! {0} to {4}";
            }

            // get the prefix
            var textPrefix = GetAppendText(TextAppendType.Prefix);

            // get the suffix
            var textSuffix = GetAppendText(TextAppendType.Suffix);

            // format the text with prefix and suffix to [PREFIX]LocalizedText[SUFFIX]
            textMain = FormatText(textPrefix + textMain + textSuffix);

            return textMain;
        } 
        #endregion
    }

    [MarkupExtensionReturnType(typeof(string))]
    public class LocTextLowerExtension : LocTextExtension
    {
        #region Constructors
        public LocTextLowerExtension()
        { }
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
            return target?.ToLower(GetForcedCultureOrDefault()) ?? string.Empty;
        }
        #endregion
    }

    [MarkupExtensionReturnType(typeof(string))]
    public class LocTextUpperExtension : LocTextExtension
    {
        #region Constructors
        public LocTextUpperExtension()
        { }
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
            return target?.ToUpper(GetForcedCultureOrDefault()) ?? string.Empty;
        }
        #endregion
    }

    [MarkupExtensionReturnType(typeof(System.Windows.Thickness))]
    public class LocThicknessExtension : LocExtension
    {
        public LocThicknessExtension()
        { }
        public LocThicknessExtension(string key) : base(key) { }
    }

#pragma warning restore 1591
}
