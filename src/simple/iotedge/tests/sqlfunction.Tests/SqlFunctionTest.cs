using Xunit;
using Industrial.Service.Bus;
using System.IO;
using System.Text;
using System.Linq;

namespace sqlfunction.Tests
{
    public class SqlFunctionTest
    {
        private static readonly string _input;

        static SqlFunctionTest()
        {
            _input = Encoding.UTF8.GetString(File.ReadAllBytes("testInput.txt"));
        }

        [Fact]
        public void ParseMessageTest()
        {
            var message = SqlFunction.Parse(_input);

            var actualValues = message.Data.Contents.SelectMany(x => x.Data).SelectMany(y => y.Values).Select(m => m.Value);

            var expectedValues = new[] { "57413", "1781" };

            Assert.Collection(
                actualValues,
                actualItem1 => Assert.Equal(expectedValues[0], actualItem1),
                actualItem2 => Assert.Equal(expectedValues[1], actualItem2));
        }
    }
}
