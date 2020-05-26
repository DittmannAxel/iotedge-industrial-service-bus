using System;
using Xunit;
using Industrial.Service.Bus;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace sqlfunction.Tests
{
    public class SqlFunctionTest
    {
        private static readonly IConfiguration _config;

        static SqlFunctionTest()
        {
            _config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", true, true)
               .Build();
        }

        [Fact]
        public void ParseMessageTest()
        {
            var input = _config["Message"];

            var message = SqlFunction.Parse(input);

            Assert.NotNull(message);
        }
    }
}
