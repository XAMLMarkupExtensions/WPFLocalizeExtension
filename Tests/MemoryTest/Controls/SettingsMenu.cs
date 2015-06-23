// -----------------------------------------------------------------------
// <copyright file="SettingsMenu.cs" company="APD Communications Limited"> 
//  Copyright ©2013 APD Communications Limited
// </copyright>
// -----------------------------------------------------------------------
namespace MemoryTest.Controls
{
	using System.Windows;
	using System.Windows.Controls;

	/// <summary>
	/// Interaction logic for SettingsMenu
	/// </summary>
	public class SettingsMenu : ContentControl
	{
		/// <summary>
		/// The menu content property
		/// </summary>
		public static readonly DependencyProperty MenuContentProperty =
			 DependencyProperty.Register(
			 "MenuContent",
			 typeof(FrameworkElement),
			 typeof(SettingsMenu),
			 new PropertyMetadata(null));

		/// <summary>
		/// The is search enabled property
		/// </summary>
		public static readonly DependencyProperty IsSearchEnabledProperty =
			 DependencyProperty.Register(
			 "IsSearchEnabled",
			 typeof(bool),
			 typeof(SettingsMenu),
			 new PropertyMetadata(true));

		/// <summary>
		/// The is settings enabled property
		/// </summary>
		public static readonly DependencyProperty IsSettingsEnabledProperty =
			 DependencyProperty.Register(
			 "IsSettingsEnabled",
			 typeof(bool),
			 typeof(SettingsMenu),
			 new PropertyMetadata(true));

		/// <summary>
		/// The filter indicator visibility property
		/// </summary>
		public static readonly DependencyProperty FilterIndicatorVisibilityProperty =
			DependencyProperty.Register(
			"FilterIndicatorVisibility",
			typeof(Visibility),
			typeof(SettingsMenu),
			new PropertyMetadata(Visibility.Collapsed));

		/// <summary>
		/// The search text property
		/// </summary>
		public static readonly DependencyProperty SearchTextProperty =
			DependencyProperty.Register(
			"SearchText",
			typeof(string),
			typeof(SettingsMenu),
			new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		/// <summary>
		/// Gets or sets the content of the menu.
		/// </summary>
		/// <value>
		/// The content of the menu.
		/// </value>
		public FrameworkElement MenuContent
		{
			get { return (FrameworkElement)GetValue(MenuContentProperty); }
			set { this.SetValue(MenuContentProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether [is search enabled].
		/// </summary>
		/// <value>
		///   <c>true</c> if [is search enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool IsSearchEnabled
		{
			get { return (bool)GetValue(IsSearchEnabledProperty); }
			set { this.SetValue(IsSearchEnabledProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether [is settings enabled].
		/// </summary>
		/// <value>
		///   <c>true</c> if [is settings enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool IsSettingsEnabled
		{
			get { return (bool)GetValue(IsSettingsEnabledProperty); }
			set { this.SetValue(IsSettingsEnabledProperty, value); }
		}

		/// <summary>
		/// Gets or sets the filter indicator visibility.
		/// </summary>
		/// <value>
		/// The filter indicator visibility.
		/// </value>
		public Visibility FilterIndicatorVisibility
		{
			get { return (Visibility)GetValue(FilterIndicatorVisibilityProperty); }
			set { this.SetValue(FilterIndicatorVisibilityProperty, value); }
		}

		/// <summary>
		/// Gets or sets the search text.
		/// </summary>
		/// <value>
		/// The search text.
		/// </value>
		public string SearchText
		{
			get { return (string)GetValue(SearchTextProperty); }
			set { this.SetValue(SearchTextProperty, value); }
		}
	}
}
