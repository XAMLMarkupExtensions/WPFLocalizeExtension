using System.Diagnostics;
using System.Windows;

namespace XamlLocalizationTest
{
  /// <summary>
  /// Implements the ResTextsDuplicateKeysDifferentValuesKeyValueWindow.
  /// </summary>
  public partial class ResTextsDuplicateKeysDifferentValuesKeyValueWindow : Window
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ResTextsDuplicateKeysDifferentValuesKeyValueWindow"/> class.
    /// </summary>
    public ResTextsDuplicateKeysDifferentValuesKeyValueWindow()
    {
      this.InitializeComponent();
    }

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="ResTextsDuplicateKeysDifferentValuesKeyValueWindow"/> is reclaimed by garbage collection.
    /// </summary>
    ~ResTextsDuplicateKeysDifferentValuesKeyValueWindow()
    {
      Debug.WriteLine("WINDOW destructed!");
    }
  }
}