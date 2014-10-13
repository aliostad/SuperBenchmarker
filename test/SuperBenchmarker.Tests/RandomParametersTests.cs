using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace SuperBenchmarker.Tests
{
    public class RandomParametersTests
    {

        [Theory]
        [InlineData("{{{MyField:RAND_STRING}}}", true)]
        [InlineData("{{{MyField:RAND_INTEGER}}}", true)]
        [InlineData("{{{MyField:RAND_INTEGER:[100:200]}}}", true)]
        [InlineData("{{{MyField:RAND_DOUBLE:[1:20000]}}}", true)]
        [InlineData("{{{MyField:RAND_DATE}}}", true)]
        [InlineData("{{{MyField:RAND_DATETIME:[2014-10-11:2014-10-15]}}}", true)]
        [InlineData("{{{MyField:RAND_DATETIMEOFFSET}}}", true)]
        [InlineData("{{{MyField:RAND_DATETIMEOFFSET:[2001-10-11:2014-10-15]}}}", true)]
        [InlineData("{{{MyField:RAND_DATETIMEOFFSETS}}}", false)]
        public void ShouldParse(string input, bool success)
        {
            var randomValueProvider = new RandomValueProvider();
            var tokenisedString = new TokenisedString(input);
            var analyseThis = randomValueProvider.AnalyseThis(tokenisedString.Tokens[0].Name);
            Assert.Equal(success, analyseThis!=null);
            
            if(analyseThis!=null)
                Console.WriteLine(analyseThis());
        }
    }
}
