using Microsoft.Azure.Devices.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using RabbitMQManager.RabbitMQConfigs;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQManager.Tests
{
    [TestClass]
    public class RabbitMQManagerTests
    {
        private const string RaabitMQConfig =
            @"{
                 'hostname': '10.0.2.8',
                 'port': 15672,
                 'username': 'guest',
                 'password': 'pass',
                 'federationConfig': 
                 {
                    'federatedExchange': 'opc-data',
                    'upstreams': {'upstream1':{'uri':'amqp://guest:guest@10.0.2.6:25672','expires':3600000}},
                    'policies': {'opc-data': { 'pattern':'^opc-data$', 'definition':{'federation-upstream-set':'all'}, 'apply-to':'exchanges'}}                 }
            }";

        [TestMethod]
        public void Can_Successfuly_Deserialize_Config()
        {
            var config = JsonConvert.DeserializeObject<RabbitMQConfig>(RaabitMQConfig);

            Assert.AreEqual("10.0.2.8", config.Hostname);
            Assert.AreEqual("guest", config.UserName);
            Assert.AreEqual("pass", config.Password);
            Assert.AreEqual(15672, config.Port);
            Assert.AreEqual("upstream1", config.FederationConfig.Upstreams.First().Key);
            Assert.AreEqual("amqp://guest:guest@10.0.2.6:25672", config.FederationConfig.Upstreams.First().Value.Uri.OriginalString);
            Assert.AreEqual(3600000, config.FederationConfig.Upstreams.First().Value.ExpirationTimeOut);
            Assert.AreEqual("opc-data", config.FederationConfig.Policies.First().Key);
            Assert.AreEqual("^opc-data$", config.FederationConfig.Policies.First().Value.Pattern);
            Assert.AreEqual("all", config.FederationConfig.Policies.First().Value.Definition["federation-upstream-set"]);
            Assert.AreEqual("exchanges", config.FederationConfig.Policies.First().Value.ApplyTo);
        }

        [TestMethod]
        public async Task Request_To_Setup_Federation_Has_Correct_Body()
        {
            // Arrange
            const string expextedUpstreamRequestBody = "{\"value\":{\"uri\":\"amqp://guest:guest@10.0.2.6:25672\",\"expires\":3600000}}";
            const string expectedPolicyRequestBody = "{\"pattern\":\"^opc-data$\",\"definition\":{\"federation-upstream-set\":\"all\"},\"apply-to\":\"exchanges\"}";
            var rabbitMQConfig = @"{'rabbitMQConfig':" + RaabitMQConfig + "}";

            var moduleTwin = new Twin(new TwinProperties 
            { 
                Desired = new TwinCollection(rabbitMQConfig)
            });

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(async (HttpRequestMessage request, CancellationToken cancellationToken) => 
                {
                    if (request.RequestUri.LocalPath == "/api/parameters/federation-upstream///upstream1")
                    {
                        var body = await request.Content.ReadAsStringAsync();
                        Assert.AreEqual(expextedUpstreamRequestBody, body);

                        return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                    }
                    else if(request.RequestUri.LocalPath == "/api/policies///opc-data")
                    {
                        var body = await request.Content.ReadAsStringAsync();
                        Assert.AreEqual(expectedPolicyRequestBody, body);
                        return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                    }

                    throw new NotImplementedException();
                });

            var target = new RabbitMQManager(new HttpClient(mockHttpMessageHandler.Object), new RabbitMQManagerInstrumentation());

            await target.ApplyConfigAsync(moduleTwin);
        }
    }
}
