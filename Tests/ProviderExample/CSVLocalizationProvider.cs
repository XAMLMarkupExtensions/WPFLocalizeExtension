#region Copyright information
// <copyright file="CSVLocalizationProvider.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace ProviderExample
{
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows;
    using System;
    using System.Reflection;
    using System.IO;
    using System.Windows.Media;
    using WPFLocalizeExtension.Providers;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// A localization provider for comma separated files
    /// </summary>
    public class CSVLocalizationProvider : FrameworkElement, ILocalizationProvider
    {
        private string fileName = "";
        /// <summary>
        /// The name of the file without an extension.
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set
            {
                if (fileName != value)
                {
                    fileName = value;

                    this.AvailableCultures.Clear();

                    var appPath = GetWorkingDirectory();
                    var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

                    foreach (var c in cultures)
                    {
                        var csv = Path.Combine(appPath, this.FileName + "." + c.Name + ".csv");
                        if (File.Exists(csv))
                            this.AvailableCultures.Add(c);
                    }

                    OnProviderChanged();
                }
            }
        }

        private bool hasHeader = false;
        /// <summary>
        /// A flag indicating, if it has a header row.
        /// </summary>
        public bool HasHeader
        {
            get { return hasHeader; }
            set { hasHeader = value; OnProviderChanged(); }
        }

        /// <summary>
        /// Raise a <see cref="ILocalizationProvider.ProviderChanged"/> event.
        /// </summary>
        private void OnProviderChanged()
        {
            if (ProviderChanged != null)
                ProviderChanged(this, new ProviderChangedEventArgs(null));
        }

        /// <summary>
        /// Calls the <see cref="ILocalizationProvider.ProviderError"/> event.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="key">The key.</param>
        /// <param name="message">The error message.</param>
        private void OnProviderError(DependencyObject target, string key, string message)
        {
            if (ProviderError != null)
                ProviderError(this, new ProviderErrorEventArgs(target, key, message));
        }

        /// <summary>
        /// Get the working directory, depending on the design mode or runtime.
        /// </summary>
        /// <returns>The working directory.</returns>
        private string GetWorkingDirectory()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var dte = (EnvDTE.DTE)Marshal.GetActiveObject("VisualStudio.DTE.10.0");
                var sb = dte.Solution.SolutionBuild;
                string msg = "";

                foreach (String item in (Array)sb.StartupProjects)
                {
                    msg += item;
                }
                
                EnvDTE.Project startupProj = dte.Solution.Item(msg);

                return (Path.GetDirectoryName(startupProj.FullName));
            }
            else
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Get the localized object.
        /// </summary>
        /// <param name="key">The key to the value.</param>
        /// <param name="target">The target object.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
        public object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture)
        {
            string ret = null;

            // Try to get the culture specific file.
            var appPath = GetWorkingDirectory();
            var csvPath = "";

            while (culture != CultureInfo.InvariantCulture)
            {
                csvPath = Path.Combine(appPath, this.FileName + (String.IsNullOrEmpty(culture.Name) ? "" : "." + culture.Name) + ".csv");

                if (File.Exists(csvPath))
                    break;

                culture = culture.Parent;
            }

            if (!File.Exists(csvPath))
            {
                // Take the invariant culture.
                csvPath = Path.Combine(appPath, this.FileName + ".csv");

                if (!File.Exists(csvPath))
                {
                    OnProviderError(target, key, "A file for the provided culture " + culture.EnglishName + " does not exist at " + csvPath + ".");
                    return null;
                }
            }

            // Open the file.
            using (var reader = new StreamReader(csvPath, Encoding.UTF8))
            {
                // Skip the header if needed.
                if (this.HasHeader && !reader.EndOfStream)
                    reader.ReadLine();

                // Read each line and split it.
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(";".ToCharArray());

                    if (parts.Length < 2)
                        continue;

                    // Check the key (1st column).
                    if (parts[0] != key)
                        continue;

                    // Get the value (2nd column).
                    ret = parts[1];
                    break;
                }
            }

            // Nothing found -> Raise the error message.
            if (ret == null)
                OnProviderError(target, key, "The key does not exist in " + csvPath + ".");

            return ret;
        }

        /// <summary>
        /// An observable list of available cultures.
        /// </summary>
        private ObservableCollection<CultureInfo> availableCultures = null;
        public ObservableCollection<CultureInfo> AvailableCultures
        {
            get
            {
                if (availableCultures == null)
                    availableCultures = new ObservableCollection<CultureInfo>();

                return availableCultures;
            }
        }

        /// <summary>
        /// Gets fired when the provider changed.
        /// </summary>
        public event ProviderChangedEventHandler ProviderChanged;

        /// <summary>
        /// An event that is fired when an error occurred.
        /// </summary>
        public event ProviderErrorEventHandler ProviderError;

        /// <summary>
        /// An event that is fired when a value changed.
        /// </summary>
        public event ValueChangedEventHandler ValueChanged;
    }
}
