using System.Text;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace artm.demo
{
    public class GetVehiclePositions
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public GetVehiclePositions(ILoggerFactory loggerFactory, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = loggerFactory.CreateLogger<GetVehiclePositions>();
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [Function("GetVehiclePositions")]
        public async Task Run([TimerTrigger("0 */2 * * * *")] MyInfo myTimer)
        {
            try
            {
                var message = await GetData();
                _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now} " + message);
                await SendToIoTHub(message);
                _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                throw;
            }
        }

        
        private async Task<JsonArray> GetData()
        {
            using var httpClient = _httpClientFactory.CreateClient();           
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await httpClient.GetAsync("https://api.translink.ca/rttiapi/v1/buses?ApiKey=" + _configuration["ApiKey"]);
            //var data = await response.Content.ReadAsByteArrayAsync();
            var data = await response.Content.ReadFromJsonAsync<JsonNode>();           
            return data.AsArray();
        }
        
        private async Task SendToIoTHub(JsonArray message)
        {
            string deviceId = _configuration["DeviceId"];
            string deviceKey = _configuration["DeviceKey"];
            string iotHubHostName = _configuration["IoTHubHostName"];
            var deviceAuthentication = new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey);

            using var deviceClient = DeviceClient.Create(iotHubHostName, deviceAuthentication, TransportType.Mqtt);
            
            var positions = message.Select(x =>  new Message(Encoding.UTF8.GetBytes(x.ToString())));       
            await deviceClient.SendEventBatchAsync(positions);
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
