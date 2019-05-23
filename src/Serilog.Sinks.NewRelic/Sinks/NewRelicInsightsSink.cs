using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.NewRelic.Sinks
{
    public class NewRelicInsightsSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly NewRelicConfigurationOptions _configurationOptions;
        private readonly HttpClient _httpClient;

        public NewRelicInsightsSink(
            IFormatProvider formatProvider, 
            NewRelicConfigurationOptions configurationOptions,
            HttpClient httpClient)
        {
            _formatProvider = formatProvider;
            _configurationOptions = configurationOptions;
            _httpClient = httpClient;
        }
        
        public async void Emit(LogEvent logEvent)
        {
            var insightsEvent = new NewRelicEvent
            {
                 Data = logEvent.RenderMessage(_formatProvider),
                 Timestamp = DateTime.UtcNow,
                 EventType = "MyEvent",
                 ApplicationName = _configurationOptions.ApplicationName,
                 EnvironmentName = _configurationOptions.EnvironmentName
            };
            
            if (logEvent.Properties.TryGetValue("CorrelationId", out var propertyValue))
            {
                insightsEvent.CorrelationId = propertyValue.ToString();
            }

            var message = JsonConvert.SerializeObject(insightsEvent, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            
            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Post, 
                string.Format(_configurationOptions.NewRelicBaseUri, _configurationOptions.AccountId));
            httpRequestMessage.Headers.Add("X-Insert-Key", _configurationOptions.LicenseKey);

            using (var content = new StringContent(message))
            {
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = await _httpClient.SendAsync(httpRequestMessage); //.GetAwaiter().GetResult();
            }
        }
    }

    class NewRelicEvent
    {
        public string EventType { get; set; }
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string ApplicationName { get; set; }
        public string EnvironmentName { get; set; }
        public string CorrelationId { get; set; }
    }
}