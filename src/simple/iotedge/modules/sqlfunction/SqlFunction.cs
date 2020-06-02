using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EdgeHub;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using DeviceMessage = Microsoft.Azure.Devices.Client.Message;
using System.IO;

namespace Industrial.Service.Bus
{
    public static class SqlFunction
    {
        private static readonly IConfiguration _config;

        static SqlFunction()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();
        }

        public static Message Parse(string input)
        {
            input = input.Replace("\'{", "{").Replace("}\'", "}");

            return JsonConvert.DeserializeObject<Message>(input);
        }

        [FunctionName("SqlFunction")]
        public static async Task StoreMessageAsync([EdgeHubTrigger("input1")] DeviceMessage input, ILogger logger)
        {
            var inputMessage = Encoding.UTF8.GetString(input.GetBytes());

            if (string.IsNullOrEmpty(inputMessage))
            {
                logger.LogInformation("Received empty message!");

                return;
            }

            logger.LogInformation($"Received message: {inputMessage}");

            var message = Parse(inputMessage);

            var measurementData = new List<string>();

            foreach (var measurement in message.Data.Contents)
            {
                foreach (var data in measurement.Data)
                {
                    foreach (var value in data.Values)
                    {
                        measurementData.Add($"('{message.Id}','{message.Source}','{message.Type}','{message.SpecVersion}','{message.DataContentType}','{measurement.HwId}','{value.Address}','{value.Value}','{data.SourceTimestamp}')");
                    }
                }
            }

            var insertStatement = $"INSERT INTO dbo.PowerMeasurements VALUES {String.Join(",", measurementData)};";

            using (var connection = new SqlConnection(_config["DatabaseConnection"]))
            {
                connection.Open();

                using (var cmd = new SqlCommand(insertStatement, connection))
                {
                    var rows = await cmd.ExecuteNonQueryAsync();

                    logger.LogInformation($"{rows} rows were updated");
                }
            }
        }
    }
}