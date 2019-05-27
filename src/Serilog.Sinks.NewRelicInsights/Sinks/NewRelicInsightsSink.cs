using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog.Events;
using Serilog.Debugging;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.NewRelicInsights.Sinks
{
    public class NewRelicInsightsSink : PeriodicBatchingSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly NewRelicConfigurationOptions _configurationOptions;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public NewRelicInsightsSink(
            IFormatProvider formatProvider,
            NewRelicConfigurationOptions configurationOptions)
            : base(batchSizeLimit: 1, period: TimeSpan.FromSeconds(1), queueLimit: 1)
        {
            _formatProvider = formatProvider;
            _configurationOptions = configurationOptions;
            _httpClient = GetHttpClientFromFactory();
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
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
                LogLevel = logEvent.Level.ToString(),
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

            var message = JsonConvert.SerializeObject(insightsEvent, _jsonSerializerSettings);

            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                string.Format(_configurationOptions.NewRelicBaseUri, _configurationOptions.AccountId));
            httpRequestMessage.Headers.Add("X-Insert-Key", _configurationOptions.LicenseKey);

            HttpResponseMessage response;
            try
            {
                using (var content = new StringContent(message))
                {
                    httpRequestMessage.Content = content;
                    response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception exception)
            {
                SelfLog.WriteLine("Unable to post to NewRelic due to the following error: {0}. Have you configured the logger?", exception.Message);
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

    internal struct NewRelicEvent
    {
        public string EventType { get; set; }
        public string LogLevel { get; set; }
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string ApplicationName { get; set; }
        public string EnvironmentName { get; set; }
        public string CorrelationId { get; set; }
    }
}