using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;

// ATTENTION!!! Don't forget to add following application settings in your Azure Function App
// From Azure Portal => Your Azure Function App => Configuration => Application settings => New application settings and add the following 2 settings:
// AzureIOTHubConnectionString
// AzureSignalRConnectionString

namespace Company.Function
{
    public class FranmerRealTimeLogistic
    {
        private static HttpClient client = new HttpClient();


        [FunctionName("messages")]
        public static async Task RunAsync(
      [IoTHubTrigger("messages/events", Connection = "AzureIOTHubConnectionString")] EventData message,
      [SignalR(HubName = "maphub")] IAsyncCollector<SignalRMessage> signalRMessages, ILogger log)

        {

            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");
            await signalRMessages.AddAsync(new SignalRMessage
            {
                Target = "newVehicleData",
                Arguments = new[] { Encoding.UTF8.GetString(message.Body.Array) }
            });
        }


        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
          [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
          [SignalRConnectionInfo(HubName = "maphub")] SignalRConnectionInfo connectionInfo,
          ILogger log)
        {
            return connectionInfo;
        }
    }
}