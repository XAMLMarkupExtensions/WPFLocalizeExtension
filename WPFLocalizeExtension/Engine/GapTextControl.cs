using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using XAMLMarkupExtensions.Base;

namespace WPFLocalizeExtension.Engine
{
    //TODO: proper handling of \n in string contents
    [TemplatePart(Name = PART_TextBlock, Type = typeof(TextBlock))]
    public class GapTextControl : Control
    {
        #region Dependency Properties

        /// <summary>
        /// This property is the string that may contain gaps for controls.
        /// </summary>
        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register(
            "FormatString",
            typeof (string),
            typeof (GapTextControl),
            new UIPropertyMetadata(string.Empty, OnFormatStringChanged));

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
            "IgnoreLessGaps",
            typeof (bool),
            typeof (GapTextControl),
            new UIPropertyMetadata(false));

        /// <summary>
        /// If this property is true, any FormatString that refers to the same string item multiple times produces an exception.
        /// </summary>
        public static readonly DependencyProperty IgnoreDuplicateStringReferencesProperty = DependencyProperty.Register(
            "IgnoreDuplicateStringReferences",
            typeof(bool),
            typeof(GapTextControl),
            new UIPropertyMetadata(true));

        /// <summary>
        /// If this property is true, any FormatString that refers to the same control item multiple times produces an exception.
        /// </summary>
        public static readonly DependencyProperty IgnoreDuplicateControlReferencesProperty = DependencyProperty.Register(
            "IgnoreDuplicateControlReferences",
            typeof(bool),
            typeof(GapTextControl),
            new UIPropertyMetadata(false));

        /// <summary>
        /// property that stores the items to be inserted into the gaps.
        /// any item that can be inserted as such into the TextBox get's inserted itself. 
        /// All other items are converted to Text using their ToString() implementation.
        /// </summary>
        public static readonly DependencyProperty GapsProperty = DependencyProperty.Register(
            "Gaps",
            typeof(ObservableCollection<object>),
            typeof(GapTextControl),
            new UIPropertyMetadata(default(ObservableCollection<object>), OnGapsChanged));

        private static void OnGapsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            // TODO: make sure there's an event handler on CollectionChanged!

            // re-assemble children:
            if (dependencyPropertyChangedEventArgs.OldValue != dependencyPropertyChangedEventArgs.NewValue)
            {
                var self = (GapTextControl) dependencyObject;
                self.OnContentChanged();
            }
        }

        #endregion

        #region constructors

        static GapTextControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GapTextControl),
                new FrameworkPropertyMetadata(typeof(GapTextControl)));
        }

        public GapTextControl()
        {
            this.Gaps = new ObservableCollection<object>();
            this.Gaps.CollectionChanged += (sender, args) => this.OnContentChanged();
        }
        #endregion

        #region properties matching DependencyProperties

        public string FormatString
        {
            get
            {
                return (string) GetValue(FormatStringProperty);
            }
            set
            {
                SetValue(FormatStringProperty, value);
            }
        }

        public bool IgnoreLessGaps {
            get { return (bool) GetValue(IgnoreLessGapsProperty); }
            set { SetValue(IgnoreLessGapsProperty, value);}
        }

        public bool IgnoreDuplicateStringReferences
        {
            get { return (bool) GetValue(IgnoreDuplicateStringReferencesProperty); }
            set { SetValue(IgnoreDuplicateStringReferencesProperty, value); }
        }
        public bool IgnoreDuplicateControlReferences {
            get { return (bool) GetValue(IgnoreDuplicateControlReferencesProperty); }
            set { SetValue(IgnoreDuplicateControlReferencesProperty, value); }
        }

        public ObservableCollection<object> Gaps
        {
            get { return (ObservableCollection<object>) GetValue(GapsProperty); }
            set { SetValue(GapsProperty, value); }
        }

        #endregion

        #region constants

        /// <summary>
        /// Pattern to split the FormatString, see https://github.com/SeriousM/WPFLocalizationExtension/issues/78#issuecomment-163023915 for documentation ( TODO!!!)
        /// </summary>
        public const string RegexPattern = @"(.*?){(\d*)}"; 
        #endregion

        #region constants for TemplateParts

        // ReSharper disable once InconsistentNaming
        private const string PART_TextBlock = "PART_TextBlock";

        #endregion

        #region Sub-Controls

        private TextBlock theTextBlock = new TextBlock();
        
        #endregion

        #region DependencyProperty changed event handlers

        private static void OnFormatStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                var self = (GapTextControl) d;
                self.OnContentChanged();
            }
        }

        public enum InlineType
        {
            Inline,
            // ReSharper disable once InconsistentNaming
            UIElement,
            String
        }

        private void OnContentChanged()
        {
            //re-arrange the children:
            this.theTextBlock.Inlines.Clear();
            
            if (this.FormatString != null)
            {
                var matchedUpToIndex = 0;

                // 1) determine which items are to be used as string and which are to be inserted as controls:
                // allowed according to https://msdn.microsoft.com/de-de/library/system.windows.documents.inlinecollection%28v=vs.110%29.aspx are
                // Inline, String (creates an implicit Run), UIElement (creates an implicit InlineUIContainer with the supplied UIElement inside), 
                if (this.Gaps != null)
                {
                    List<Tuple<InlineType, object>> classifiedGaps = new List<Tuple<InlineType, object>>(this.Gaps.Count);

                    foreach (var gap in this.Gaps)
                    {
                        // inlines are directly allowed
                        var inlineGap = gap as Inline;
                        if (inlineGap != null)
                        {
                            // inline gaps are basically strings
                            classifiedGaps.Add(new Tuple<InlineType, object>(InlineType.Inline, gap));
                        }
                        else
                        {
                            var uiElementGap = gap as UIElement;
                            if (uiElementGap != null)
                            {
                                // this is a control and should be added as such
                                classifiedGaps.Add(new Tuple<InlineType, object>(InlineType.UIElement, gap));
                            }
                            else
                            {
                                // cast it to string and add an implicit run 
                                // TODO: proove that this done implicitly anyways by string.Format()
                                classifiedGaps.Add(new Tuple<InlineType, object>(InlineType.String, gap.ToString()));
                            }
                        }
                    }


                    //string[] controlPlaceholders = controlGapIndices.Select(i => string.Format("{0}", i)).ToArray();
                    var match = Regex.Match(this.FormatString, RegexPattern);
                    
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
                        this.theTextBlock.Inlines.Add(string.Format(formatStringPartial, this.Gaps));
                        // 2) the item encoded in the placeholder:
                        try
                        {
                            var necessaryTuple = classifiedGaps[itemIndex];

                            switch (necessaryTuple.Item1)
                            {
                                case InlineType.UIElement:
                                    this.theTextBlock.Inlines.Add((UIElement) this.Gaps[itemIndex]);
                                    break;
                                case InlineType.String:
                                    this.theTextBlock.Inlines.Add((string) this.Gaps[itemIndex]);
                                    break;
                                case InlineType.Inline:
                                    this.theTextBlock.Inlines.Add((Inline) this.Gaps[itemIndex]);
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            // break for now
                        }
                        

                    }
                }

                // add the remaining part:

                this.theTextBlock.Inlines.Add(string.Format(FormatString.Substring(matchedUpToIndex), this.Gaps));
                //this.theTextBlock.InvalidateVisual();
                this.InvalidateVisual();
                

            }
            else
            {
                throw new Exception("property FormatString is not a string!");
            } 
        }

        #endregion

        #region public methods and operators 

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.AttachToVisualTree();
        }

        #endregion

        #region methods

        private void AttachToVisualTree()
        {
            // this.theTextBlock = this.GetTemplateChild(PART_TextBlock) as TextBlock;

            if (this.Template != null)
            {
                TextBlock textBlock = this.Template.FindName(PART_TextBlock, this) as TextBlock;
                if (textBlock != this.theTextBlock)
                {
                    //Firstly you have to unhook existing handler
                    //if (MB != null)
                    //{
                    //    MB.MouseEnter -= new MouseEventHandler(MB_MEnter);
                    //    MB.MouseLeave -= new MouseEventHandler(MB_MLeave);
                    //}
                    this.theTextBlock = textBlock;
                    this.OnContentChanged();
                    //if (MB != null)
                    //{
                    //    // Now we have to Add a default basecolor
                    //    MB.Background = new LinearGradientBrush(this.Color, this.Color, .5);
                    //    MB.MouseEnter += new MouseEventHandler(MB_MEnter);
                    //    MB.MouseLeave += new MouseEventHandler(MB_MLeave);
                    //}
                }
            }
        }

        #endregion
    }
}