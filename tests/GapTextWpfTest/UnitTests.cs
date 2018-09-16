using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using Xunit;

namespace GapTextWpfTest
{
    public class UnitTests
    {
        [Theory]
        [InlineData("Dies ist ein {0}", new[]{0} )]
        [InlineData("Ein {0} Text mit {2} oder 3 Platzhaltern{1}", new[] {0, 2})]
        [InlineData("Ein Platzhalter {0} ohne und einer {1:10} mit Padding-Angabe", new[] {0})]
        [InlineData("Ein Platzhalter mit {0d} Formatierungsvorgabe und einer {1} ohne.", new[] {1})]
        [InlineData("Kein Platzhalter", new int[] {})]

        public void SplitFormatStringTest(string formatString, int[] controlPlaceholderIndices)
        {
            var pattern = @"(.*?){(\d*)}"; // ends after parameter, non-greedy

            Match match = Regex.Match(formatString, pattern);
            while (match.Success)
            {
                // Handle match here...
                var group0 = match.Groups[0];
                var group1 = match.Groups[1];
                var group2 = match.Groups[2];
            }
            match = match.NextMatch();
        }

        [Fact]
        public void RunToString()
        {
            var run = new Run();
            run.Text = "foo";
            Assert.Equal("foo", run.ToString());
        }
    }
}
