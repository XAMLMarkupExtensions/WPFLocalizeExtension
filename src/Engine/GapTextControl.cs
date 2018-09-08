#region Copyright information
// <copyright file="GapTextControl.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Peter Wendorff</author>
// <author>Uwe Mayer</author>
#endregion

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace WPFLocalizeExtension.Engine
{
    /// <summary>
    /// A gap text control.
    /// </summary>
    [TemplatePart(Name = PART_TextBlock, Type = typeof(TextBlock))]
    public class GapTextControl : Control
    {
        #region Dependency Properties
        /// <summary>
        /// This property is the string that may contain gaps for controls.
        /// </summary>
        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register(
            nameof(FormatString),
            typeof(string),
            typeof(GapTextControl),
            new PropertyMetadata(string.Empty, OnFormatStringChanged));

        /// <summary>
        /// If this property is set to true there is no error thrown 
        /// when the FormatString contains less gaps than placeholders are available.
        /// Missing placeholders for available elements may be a problem, 
        /// as something else may refer to the element in a binding e.g. by name, 
        /// but the element is not available in the visual tree.
        /// 
        /// As an example consider a submit button would be missing due to a missing placeholder in the FormatString.
        /// </summary>
        public static readonly DependencyProperty IgnoreLessGapsProperty = DependencyProperty.Register(
            nameof(IgnoreLessGaps),
            typeof(bool),
            typeof(GapTextControl),
            new PropertyMetadata(false));

        /// <summary>
        /// If this property is true, any FormatString that refers to the same string item multiple times produces an exception.
        /// </summary>
        public static readonly DependencyProperty IgnoreDuplicateStringReferencesProperty = DependencyProperty.Register(
            nameof(IgnoreDuplicateStringReferences),
            typeof(bool),
            typeof(GapTextControl),
            new PropertyMetadata(true));

        /// <summary>
        /// If this property is true, any FormatString that refers to the same control item multiple times produces an exception.
        /// </summary>
        public static readonly DependencyProperty IgnoreDuplicateControlReferencesProperty = DependencyProperty.Register(
            nameof(IgnoreDuplicateControlReferences),
            typeof(bool),
            typeof(GapTextControl),
            new PropertyMetadata(false));

        /// <summary>
        /// property that stores the items to be inserted into the gaps.
        /// any item that can be inserted as such into the TextBox get's inserted itself. 
        /// All other items are converted to Text using their ToString() implementation.
        /// </summary>
        public static readonly DependencyProperty GapsProperty = DependencyProperty.Register(
            nameof(Gaps),
            typeof(ObservableCollection<object>),
            typeof(GapTextControl),
            new PropertyMetadata(default(ObservableCollection<object>), OnGapsChanged));

        private static void OnGapsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            // TODO: make sure there's an event handler on CollectionChanged!

            // re-assemble children:
            if (dependencyPropertyChangedEventArgs.OldValue != dependencyPropertyChangedEventArgs.NewValue)
            {
                var self = (GapTextControl)dependencyObject;
                self.OnContentChanged();
            }
        }
        #endregion

        #region Constructors
        static GapTextControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GapTextControl), new FrameworkPropertyMetadata(typeof(GapTextControl)));
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public GapTextControl()
        {
            Gaps = new ObservableCollection<object>();
            Gaps.CollectionChanged += (sender, args) => OnContentChanged();
        }
        #endregion

        #region Properties matching the DependencyProperties
        /// <summary>
        /// Gets or set the format string.
        /// </summary>
        public string FormatString
        {
            get => (string)GetValue(FormatStringProperty);
            set => SetValue(FormatStringProperty, value);
        }

        /// <summary>
        /// Ignore the Less Gaps
        /// </summary>
        public bool IgnoreLessGaps
        {
            get => (bool)GetValue(IgnoreLessGapsProperty);
            set => SetValue(IgnoreLessGapsProperty, value);
        }

        /// <summary>
        /// Ignore Duplicate String References
        /// </summary>
        public bool IgnoreDuplicateStringReferences
        {
            get => (bool)GetValue(IgnoreDuplicateStringReferencesProperty);
            set => SetValue(IgnoreDuplicateStringReferencesProperty, value);
        }

        /// <summary>
        /// Ignore Duplicate Control References
        /// </summary>
        public bool IgnoreDuplicateControlReferences
        {
            get => (bool)GetValue(IgnoreDuplicateControlReferencesProperty);
            set => SetValue(IgnoreDuplicateControlReferencesProperty, value);
        }

        /// <summary>
        /// Gets or sets the gap collection.
        /// </summary>
        public ObservableCollection<object> Gaps
        {
            get => (ObservableCollection<object>)GetValue(GapsProperty);
            set => SetValue(GapsProperty, value);
        }
        #endregion

        #region Constants
        /// <summary>
        /// Pattern to split the FormatString, see https://github.com/SeriousM/WPFLocalizationExtension/issues/78#issuecomment-163023915 for documentation ( TODO!!!)
        /// </summary>
        public const string RegexPattern = @"(.*?){(\d*)}";
        #endregion

        #region Constants for TemplateParts
        // ReSharper disable once InconsistentNaming
        private const string PART_TextBlock = "PART_TextBlock";
        #endregion

        #region Sub-Controls
        private TextBlock _theTextBlock = new TextBlock();
        #endregion

        #region DependencyProperty changed event handlers
        private static void OnFormatStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                var self = (GapTextControl)d;
                self.OnContentChanged();
            }
        }

        private static T DeepCopy<T>(T obj)
            where T : DependencyObject
        {
            var xaml = XamlWriter.Save(obj);
            var stringReader = new StringReader(xaml);
            var xmlTextReader = new XmlTextReader(stringReader);
            var result = (T)XamlReader.Load(xmlTextReader);

            var enumerator = obj.GetLocalValueEnumerator();
            while (enumerator.MoveNext())
            {
                var dp = enumerator.Current.Property;
                var be = BindingOperations.GetBindingExpression(obj, dp);
                if (be?.ParentBinding?.Path != null)
                    BindingOperations.SetBinding(result, dp, be.ParentBinding);
            }

            return result;
        }

        private void OnContentChanged()
        {
            // Re-arrange the children:
            _theTextBlock.Inlines.Clear();

            if (FormatString != null)
            {
                var matchedUpToIndex = 0;

                // 1) determine which items are to be used as string and which are to be inserted as controls:
                // allowed according to https://msdn.microsoft.com/de-de/library/system.windows.documents.inlinecollection%28v=vs.110%29.aspx are
                // Inline, String (creates an implicit Run), UIElement (creates an implicit InlineUIContainer with the supplied UIElement inside), 
                if (Gaps != null)
                {
                    var match = Regex.Match(FormatString, RegexPattern);

                    while (match.Success)
                    {
                        // Handle match here...
                        var wholeMatch = match.Groups[0].Value; // contains string and simple placeholder at the end.
                        var formatStringPartial = match.Groups[1].Value;
                        // has still to be formatted TODO or even better bound accordingly by lex:loc binding
                        var itemIndex = int.Parse(match.Groups[2].Value);
                        // it's secure to parse an int here as this follows from the regex.

                        matchedUpToIndex += wholeMatch.Length;

                        // get next match:
                        match = match.NextMatch();

                        // add the inlines:
                        // 1) the prefix that is formatted with the whole gaps parameters:
                        _theTextBlock.Inlines.Add(string.Format(formatStringPartial, Gaps));

                        // Check availability of a classified gap.
                        if (Gaps.Count <= itemIndex)
                            continue;
                        var gap = Gaps[itemIndex];

                        // 2) the item encoded in the placeholder:
                        try
                        {
                            if (gap is UIElement element)
                            {
                                var item = DeepCopy(element);
                                _theTextBlock.Inlines.Add(item);
                            }
                            else if (gap is Inline)
                            {
                                var item = DeepCopy((Inline)gap);
                                _theTextBlock.Inlines.Add(item);
                            }
                            else if (gap != null)
                                _theTextBlock.Inlines.Add(gap.ToString());
                        }
                        catch (Exception)
                        {
                            // break for now
                        }
                    }
                }

                // add the remaining part:
                _theTextBlock.Inlines.Add(string.Format(FormatString.Substring(matchedUpToIndex), Gaps));
                
                InvalidateVisual();
            }
            else
            {
                throw new Exception("FormatString is not a string!");
            }
        }
        #endregion

        #region Template stuff
        /// <summary>
        /// Will be called prior to display of the control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AttachToVisualTree();
        }

        private void AttachToVisualTree()
        {
            if (Template != null)
            {
                var textBlock = Template.FindName(PART_TextBlock, this) as TextBlock;
                if (textBlock != _theTextBlock)
                {
                    _theTextBlock = textBlock;
                    OnContentChanged();
                }
            }
        }
        #endregion
    }
}