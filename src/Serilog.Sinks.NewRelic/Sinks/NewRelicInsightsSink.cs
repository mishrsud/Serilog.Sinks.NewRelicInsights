using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.NewRelic.Sinks
{
    public class NewRelicInsightsSink : PeriodicBatchingSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly NewRelicConfigurationOptions _configurationOptions;
        private readonly HttpClient _httpClient;

        public NewRelicInsightsSink(
            IFormatProvider formatProvider, 
            NewRelicConfigurationOptions configurationOptions) 
                : base(batchSizeLimit: 1, period: TimeSpan.FromSeconds(1), queueLimit: 1)
        {
            _formatProvider = formatProvider;
            _configurationOptions = configurationOptions;
            _httpClient = GetHttpClientFromFactory();
        }

        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            foreach (var logEvent in events)
            {
                await PostToNewRelic(logEvent);
            }
        }

        private async Task PostToNewRelic(LogEvent logEvent)
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
                httpRequestMessage.Content = content;
                var response = await _httpClient.SendAsync(httpRequestMessage);
            }
        }
        
        private HttpClient GetHttpClientFromFactory()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddHttpClient("NewRelicSink.Handler",
                    client => { client.DefaultRequestHeaders.Add("Content-Type", "application/json"); })
                .ConfigurePrimaryHttpMessageHandler(messageHandler =>
                {
                    var handler = new HttpClientHandler();
                    if (handler.SupportsAutomaticDecompression)
                    {
                        handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    }

                    return handler;
                });
            var httpClientFactory = serviceCollection.BuildServiceProvider().GetService<IHttpClientFactory>();
            return httpClientFactory.CreateClient();
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