using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EdgeHub;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sql = System.Data.SqlClient;

namespace Functions.Samples
{
    public static class sqlFunction
    {
        [FunctionName("sqlFunction")]
        public static async Task FilterMessageAndSendMessage(
            [EdgeHubTrigger("input1")] Message messageReceived,
            [EdgeHub(OutputName = "output1")] IAsyncCollector<Message> output,
            ILogger logger)
        {
            // const int temperatureThreshold = 20;
            byte[] messageBytes = messageReceived.GetBytes();
            var messageString = System.Text.Encoding.UTF8.GetString(messageBytes);
            
            logger.LogInformation(messageString);

            if (!string.IsNullOrEmpty(messageString))
            {
                logger.LogInformation("Info: Received one non-empty message");
                // Get the body of the message and deserialize it.                
                
                //Remove char in JSON String
                messageString.Replace(@"\","");
                


                var messageBody = JsonConvert.DeserializeObject<Envelope>(messageString);
                var deserializedData = JsonConvert.DeserializeObject<Data>(messageBody.data);

                //Store the data in SQL db                
                string str = "Data Source=tcp:sql;Initial Catalog=MeasurementsDB;User ID=SA;Password=Strong!Passw0rd;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;";
                
                using (Sql.SqlConnection conn = new Sql.SqlConnection(str))
                {                    
                    conn.Open();
                    
                    foreach (var item in deserializedData.Contents)
                    {

                        foreach (var data in item.Data)
                        {

                            foreach (var value in data.Values)
                            {
                                var insertMachinePower = "INSERT INTO MeasurementsDB.dbo.PowerMeasurements VALUES ('"  + messageBody.id + "','" + messageBody.source +"','" + messageBody.type + "','" + messageBody.specversion + "','" + messageBody.datacontenttype + "','" + item.HwId +  "','" + value.Address +  "','" + value.Value +  "','" + data.SourceTimestamp.ToString() + "');";
                                using (Sql.SqlCommand cmd = new Sql.SqlCommand(insertMachinePower, conn))
                                {
                                //Execute the command and log the # rows affected.
                                    var rows = await cmd.ExecuteNonQueryAsync();
                                    logger.LogInformation($"{rows} rows were updated");
                                }
                            }
                              
                        }                      
                    }
                    
                    
                  
                    
                }

           
        }
    }
    }
   
    class Envelope {

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("source")]
        public string source { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("specversion")]
        public string specversion { get; set; }


        [JsonProperty("datacontenttype")]
        public string datacontenttype { get; set; }

        [JsonProperty("data")]
         public string data { get; set; }

    }

    class Data {

        [JsonProperty("PublishTimestamp")]
        public string PublishTimestamp { get; set; }

        [JsonProperty("Content")]
        public List<Content> Contents { get; set; }

    }

    class Content {

        [JsonProperty("HwId")]
        public string HwId { get; set; }

        [JsonProperty("Data")]
        public List<DataContent> Data { get; set; }
    }

    class DataContent {

        [JsonProperty("CorrelationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("SourceTimestamp")]
        public string SourceTimestamp { get; set; }

        [JsonProperty("Values")]
        public List<ValueS> Values { get; set; }
    }

    class ValueS {

        [JsonProperty("Displayname")]
        public string Displayname { get; set; }

        [JsonProperty("Address")]
        public string Address { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }


}
    

