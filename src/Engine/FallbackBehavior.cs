namespace WPFLocalizeExtension.Engine
{
    /// <summary>
    /// Behavior when key is not found at the localization provider.
    /// </summary>
    public enum FallbackBehavior
    {
        /// <summary>
        /// Display "Key: {key}" string.
        /// </summary>
        Default,
        
        /// <summary>
        /// Display key string itself.
        /// </summary>
        Key,
        
        /// <summary>
        /// Display an empty string.
        /// </summary>
        EmptyString
    }
}