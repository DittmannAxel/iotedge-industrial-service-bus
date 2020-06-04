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
            var message = SqlFunction.ParseMessage(_input);

            var messageData = SqlFunction.ParseData(message.Data);

            var actualValues = messageData.Contents.SelectMany(x => x.Data).SelectMany(y => y.Values).Select(m => m.Value);

            var expectedValues = new[] { "30511", "51071" };

            Assert.Collection(
                actualValues,
                actualItem1 => Assert.Equal(expectedValues[0], actualItem1),
                actualItem2 => Assert.Equal(expectedValues[1], actualItem2));
        }
    }
}
