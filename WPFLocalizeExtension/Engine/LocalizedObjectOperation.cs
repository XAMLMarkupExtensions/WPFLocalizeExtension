using System;
using System.Globalization;
using System.Reflection;

namespace WPFLocalizeExtension.Engine
{
    /// <summary>
    /// Implements the LocalizedObjectOperation.
    /// </summary>
    public static class LocalizedObjectOperation
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <param name="errorNo">The error no.</param>
        /// <returns>The resolved string or a default error string.</returns>
        public static string GetErrorMessage(int errorNo)
        {
            try
            {
                return (string) LocalizeDictionary.Instance.GetLocalizedObject<object>(
                                    LocalizeDictionary.Instance.GetAssemblyName(Assembly.GetExecutingAssembly()),
                                    "ResError",
                                    "ERR_" + errorNo,
                                    LocalizeDictionary.Instance.Culture);
            }
            catch
            {
                return "No localized ErrorMessage founded for ErrorNr: " + errorNo;
            }
        }

        /// <summary>
        /// Gets the GUI string.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        /// <returns>The resolved string or a default error string.</returns>
        public static string GetGuiString(string key)
        {
            return GetGuiString(key, LocalizeDictionary.Instance.Culture);
        }

        /// <summary>
        /// Gets the GUI string.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        /// <param name="language">The language.</param>
        /// <returns>The resolved string or a default error string.</returns>
        public static string GetGuiString(string key, CultureInfo language)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (key == string.Empty)
            {
                throw new ArgumentException("key is empty", "key");
            }

            try
            {
                return (string) LocalizeDictionary.Instance.GetLocalizedObject<object>(
                                    LocalizeDictionary.Instance.GetAssemblyName(Assembly.GetExecutingAssembly()),
                                    "ResGui",
                                    key,
                                    language);
            }
            catch
            {
                return "No localized GuiMessage founded for key '" + key + "'";
            }
        }

        /// <summary>
        /// Gets the help string.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        /// <returns>The resolved string or a default error string.</returns>
        public static string GetHelpString(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (key == string.Empty)
            {
                throw new ArgumentException("key is empty", "key");
            }

            try
            {
                return (string) LocalizeDictionary.Instance.GetLocalizedObject<object>(
                                    LocalizeDictionary.Instance.GetAssemblyName(Assembly.GetExecutingAssembly()),
                                    "ResHelp",
                                    key,
                                    LocalizeDictionary.Instance.Culture);
            }
            catch
            {
                return "No localized HelpMessage founded for key '" + key + "'";
            }
        }

        /// <summary>
        /// Gets the maintenance string.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        /// <returns>The resolved string or a default error string.</returns>
        public static string GetMaintenanceString(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (key == string.Empty)
            {
                throw new ArgumentException("key is empty", "key");
            }

            try
            {
                return (string) LocalizeDictionary.Instance.GetLocalizedObject<object>(
                                    LocalizeDictionary.Instance.GetAssemblyName(Assembly.GetExecutingAssembly()),
                                    "ResMaintenance",
                                    key,
                                    LocalizeDictionary.Instance.Culture);
            }
            catch
            {
                return "No localized MaintenanceMessage founded for key '" + key + "'";
            }
        }

        /// <summary>
        /// Gets the update agent string.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        /// <returns>The resolved string or a default error string.</returns>
        public static string GetUpdateAgentString(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (key == string.Empty)
            {
                throw new ArgumentException("key is empty", "key");
            }

            try
            {
                return (string) LocalizeDictionary.Instance.GetLocalizedObject<object>(
                                    LocalizeDictionary.Instance.GetAssemblyName(Assembly.GetExecutingAssembly()),
                                    "ResUpdateAgent",
                                    key,
                                    LocalizeDictionary.Instance.Culture);
            }
            catch
            {
                return "No localized UpdateAgentMessage founded for key '" + key + "'";
            }
        }
    }
}