using RabbitMQManager.RabbitMQConfigs;
using System;
using System.Net.Http;

namespace RabbitMQManager
{
    public interface IRabbitMQManagerInstrumentation
    {
        void ApplyingUpstreamConfiguration(string upstream);

        void AppliedUpstreamConfiguration(HttpResponseMessage response, string upstream);

        void ApplyingPolicyConfiguration(string policyName);

        void AppliedPolicyConfiguration(HttpResponseMessage response, string policyName);

        void ReceivedNewConfig(RabbitMQConfig config);

        void ConfigurationFailed(AggregateException ex, RabbitMQConfig rabbitMQConfig);

        void ConfigurationFailed(Exception ex, RabbitMQConfig rabbitMQConfig);
    }

    public class RabbitMQManagerInstrumentation : IRabbitMQManagerInstrumentation
    {
        public void ApplyingUpstreamConfiguration(string upstream)
        {
            Console.WriteLine($"Applying configuration for upstream {upstream}");
        }

        public void AppliedUpstreamConfiguration(HttpResponseMessage response, string upstream)
        {
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Applied configuration for upstream {upstream}");
            }
            else
            {
                Console.WriteLine($"Error: {response.ReasonPhrase} applying configuration for upstream {upstream}. Stautcode: {response.StatusCode}");
            }
        }

        public void ApplyingPolicyConfiguration(string policyName)
        {
            Console.WriteLine($"Applying policy configuration for federated exchange {policyName}");
        }

        public void AppliedPolicyConfiguration(HttpResponseMessage response, string policyName)
        {
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Applied policy {policyName}");
            }
            else
            {
                Console.WriteLine($"Error: {response.ReasonPhrase} applying policy {policyName}. Stautcode: {response.StatusCode}");
            }
        }

        public void ReceivedNewConfig(RabbitMQConfig rabbitMQConfig)
        {
            Console.WriteLine($"Received configuration for RabbitMQ {rabbitMQConfig.Hostname}:{rabbitMQConfig.Port}");
        }

        public void ConfigurationFailed(AggregateException ex, RabbitMQConfig rabbitMQConfig)
        {
            Console.WriteLine($"Failed to apply configuration to RabbitMQ on {rabbitMQConfig.Hostname}:{rabbitMQConfig.Port}");
            foreach (Exception innerException in ex.Flatten().InnerExceptions)
            {
                Console.WriteLine(innerException.Message);
            }
        }

        public void ConfigurationFailed(Exception ex, RabbitMQConfig rabbitMQConfig)
        {
            Console.WriteLine($"Failed to apply configuration to RabbitMQ on {rabbitMQConfig.Hostname}:{rabbitMQConfig.Port}");
            Console.WriteLine(ex);
        }
    }
}
