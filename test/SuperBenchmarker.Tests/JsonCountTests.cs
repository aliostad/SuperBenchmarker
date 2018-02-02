using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SuperBenchmarker.Tests
{

    public class JsonCountTests
    {
        [Fact]
        public void CountsCorrectlyShallow()
        {
            var text = File.ReadAllText("a.json");
            Assert.Equal(60, JsonCounter.Count(text, "recommendations"));
        }

        [Fact]
        public void CountsCorrectlyDeep()
        {
            var text = File.ReadAllText("a.json");
            Assert.Equal(4, JsonCounter.Count(text, "siba/vala"));
        }

        [Fact]
        public void ReturnsNullIfNotFound()
        {
            var text = File.ReadAllText("a.json");
            Assert.Equal(null, JsonCounter.Count(text, "recommendations/bibi"));
        }
    }
}
