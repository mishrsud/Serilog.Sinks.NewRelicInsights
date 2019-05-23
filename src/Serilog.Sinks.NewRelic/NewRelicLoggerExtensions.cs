using System;
using System.Net.Http;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.NewRelic.Sinks;

namespace Serilog.Sinks.NewRelic
{
    public static class NewRelicLoggerExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerConfiguration"></param>
        /// <param name="configurationOptions"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="formatProvider"></param>
        /// <param name="restrictedToMinimumLevel"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static LoggerConfiguration NewRelicInsights(
            this LoggerSinkConfiguration loggerConfiguration,
            NewRelicConfigurationOptions configurationOptions,
            IHttpClientFactory httpClientFactory,
            IFormatProvider formatProvider = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException(nameof(loggerConfiguration));
            var httpClient = httpClientFactory.CreateClient("NewRelicInsights.Sink");
            return loggerConfiguration.Sink(
                new NewRelicInsightsSink(formatProvider, configurationOptions, httpClient), 
                restrictedToMinimumLevel);
        }
    }
}