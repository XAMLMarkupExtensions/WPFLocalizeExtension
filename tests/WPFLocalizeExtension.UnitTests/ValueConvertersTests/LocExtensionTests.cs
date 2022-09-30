namespace WPFLocalizeExtension.UnitTests.ValueConvertersTests
{
    #region Usings
    using Xunit;
    using XAMLMarkupExtensions.Base;
    using WPFLocalizeExtension.Engine;
    using WPFLocalizeExtension.Extensions;
    #endregion
    
    /// <summary>
    /// Tests for <see cref="LocExtension" />.
    /// </summary>
    public class LocExtensionTests
    {
        #region FallbackBehavior

        private const string MISSING_KEY_RESULT = nameof(MISSING_KEY_RESULT);
        
        /// <summary>
        /// Check different behaviors when key is not found at resource provider.
        /// </summary>
        [Theory]
        [InlineData(FallbackBehavior.Default, "Key: abacaba")]
        [InlineData(FallbackBehavior.Key, "abacaba")]
        [InlineData(FallbackBehavior.EmptyString, "")]
        public void FormatOutput_SpecifiedFallbackBehavior_SpecifiedOutput(FallbackBehavior fallbackBehavior, string expectedValue)
        {
            // ARRANGE.
            const string key = "abacaba";
            
            var locExtension = new LocExtension(key);
            locExtension.FallbackBehavior = fallbackBehavior;
            var endPoint = new TargetInfo(null, null, typeof(string), -1);
            var info = new TargetInfo(null, null, typeof(string), -1);

            // ACT.
            var resultValue = locExtension.FormatOutput(endPoint, info);
            
            // ASSERT.
            Assert.Equal(expectedValue, resultValue);
        }

        /// <summary>
        /// Check if <see cref="LocalizeDictionary.MissingKeyEvent" /> specifies missing value, then <see cref="FallbackBehavior" /> is not used.
        /// </summary>
        [Theory]
        [InlineData(FallbackBehavior.Default)]
        [InlineData(FallbackBehavior.Key)]
        [InlineData(FallbackBehavior.EmptyString)]
        public void FormatOutput_MissingKeyEventHandling_FallbackBehaviorNotUsed(FallbackBehavior fallbackBehavior)
        {
            // ARRANGE.
            const string key = "abacaba";
            
            var locExtension = new LocExtension(key);
            locExtension.FallbackBehavior = fallbackBehavior;
            var endPoint = new TargetInfo(null, null, typeof(string), -1);
            var info = new TargetInfo(null, null, typeof(string), -1);

            // ACT.
            object resultValue;
            LocalizeDictionary.Instance.MissingKeyEvent += OnMissingKeyEvent;
            
            try
            {
                resultValue = locExtension.FormatOutput(endPoint, info);
            }
            finally
            {
                LocalizeDictionary.Instance.MissingKeyEvent -= OnMissingKeyEvent;
            }
            
            // ASSERT.
            Assert.Equal(MISSING_KEY_RESULT, resultValue);
        }

        /// <summary>
        /// Handle <see cref="LocalizeDictionary.MissingKeyEvent" />.
        /// </summary>
        private static void OnMissingKeyEvent(object sender, MissingKeyEventArgs e)
        {
            e.MissingKeyResult = MISSING_KEY_RESULT;
        }

        #endregion
    }
}