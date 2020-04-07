using System;
using System.Net.Http;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;

namespace RabbitMQManager
{
    enum OperationStatus
    {
        OK,
        Error
    }

    class Program
    {
        private static RabbitMQManager rabbitMqManager;
        private static HttpClient httpClient;

        static void Main(string[] args)
        {
            Run().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        static async Task Run()
        {
            var mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            var ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            httpClient = new HttpClient();
            rabbitMqManager = new RabbitMQManager(httpClient, new RabbitMQManagerInstrumentation());

            await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(async (props, ctx) =>
            {
                Console.WriteLine("Handling desired properties update!");
                await ApplyConfigAsync(ioTHubModuleClient);
            }, null);

            await ApplyConfigAsync(ioTHubModuleClient);
        }

        private async static Task ApplyConfigAsync(ModuleClient ioTHubModuleClient)
        {
            try
            {
                Console.WriteLine("Merging RabbitMQ Config is not yet implemented!");

                var moduleTwin = await ioTHubModuleClient.GetTwinAsync();
                await rabbitMqManager.ApplyConfigAsync(moduleTwin);
                await UpdateReportedProperties(ioTHubModuleClient, OperationStatus.OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await UpdateReportedProperties(ioTHubModuleClient, OperationStatus.Error);
            }
        }

        private async static Task UpdateReportedProperties(ModuleClient ioTHubModuleClient, OperationStatus status)
        {
            var reportedProperties = new TwinCollection();
            reportedProperties[Constants.RabbitMQConfigPropertyName] = status.ToString();
            await ioTHubModuleClient.UpdateReportedPropertiesAsync(reportedProperties);
        }
    }
}
