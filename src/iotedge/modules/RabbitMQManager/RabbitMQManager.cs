using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQManager.RabbitMQConfigs;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQManager
{
    public class RabbitMQManager
    {
        private readonly IRabbitMQManagerInstrumentation instrumentation;
        private readonly HttpClient httpClient;
        private RabbitMQConfig rabbitMQConfig;

        public RabbitMQManager(HttpClient httpClient, IRabbitMQManagerInstrumentation instrumentation)
        {
            this.httpClient = httpClient;
            this.instrumentation = instrumentation;
        }

        public async Task ApplyConfigAsync(Twin moduleTwin)
        {
            var desiredProperties = moduleTwin.Properties.Desired;

            if(desiredProperties.Contains(Constants.RabbitMQConfigPropertyName) && 
                desiredProperties[Constants.RabbitMQConfigPropertyName] != null)
            {
                var rabbitMQConfigAsJson = desiredProperties[Constants.RabbitMQConfigPropertyName].ToString();
                rabbitMQConfig = JsonConvert.DeserializeObject<RabbitMQConfig>(rabbitMQConfigAsJson);

                instrumentation.ReceivedNewConfig(rabbitMQConfig);

                httpClient.BaseAddress = new Uri($"http://{rabbitMQConfig.Hostname}:{rabbitMQConfig.Port}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                   "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{rabbitMQConfig.UserName}:{rabbitMQConfig.Password}")));

                await ApplyFederationConfigAsync();

                // TODO others
            }
        }

        private async Task ApplyFederationConfigAsync()
        {
            try
            {
                var federationConfig = rabbitMQConfig.FederationConfig;
                var retryPolicy = GetRetryPolicy();

                // Setup upstreams
                foreach (var upstream in federationConfig.Upstreams)
                {
                    var upstreamRequestBody = "{\"value\":" + JsonConvert.SerializeObject(upstream.Value) + "}";
                    await retryPolicy.ExecuteAsync(async () => {
                        instrumentation.ApplyingUpstreamConfiguration(upstream.Key);

                        var response = await httpClient.PutAsync(
                            $"api/parameters/federation-upstream/%2f/{upstream.Key}", new StringContent(upstreamRequestBody));

                        instrumentation.AppliedUpstreamConfiguration(response, upstream.Key);
                        return response;
                    });
                }

                // Setup policy
                foreach(var policy in federationConfig.Policies)
                {
                    var policyRequestBody = JsonConvert.SerializeObject(policy.Value);
                    await retryPolicy.ExecuteAsync(async () => {
                        instrumentation.ApplyingPolicyConfiguration(policy.Key);

                        var response = await httpClient.PutAsync(
                            $"/api/policies/%2f/{policy.Key}", new StringContent(policyRequestBody));

                        instrumentation.AppliedPolicyConfiguration(response, policy.Key);
                        return response;
                    });
                }
            }
            catch (AggregateException ex)
            {
                instrumentation.ConfigurationFailed(ex, rabbitMQConfig);
                throw;
            }
            catch(Exception ex)
            {
                instrumentation.ConfigurationFailed(ex, rabbitMQConfig);
                throw;
            }
        }

        private AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            const int maxRetryAttempts = 10;
            var pauseBetweenFailures = TimeSpan.FromSeconds(5);
            HttpStatusCode[] httpStatusCodesWorthRetrying = {
                HttpStatusCode.RequestTimeout, // 408
                HttpStatusCode.InternalServerError, // 500
                HttpStatusCode.BadGateway, // 502
                HttpStatusCode.ServiceUnavailable, // 503
                HttpStatusCode.GatewayTimeout // 504
            };

            return Polly.Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures);
        }
    }
}
