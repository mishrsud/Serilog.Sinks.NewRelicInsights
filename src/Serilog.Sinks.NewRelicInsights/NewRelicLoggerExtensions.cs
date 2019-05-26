using System;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.NewRelicInsights.Sinks;

namespace Serilog.Sinks.NewRelicInsights
{
    public static class NewRelicLoggerExtensions
    {
        /// <summary>
        /// Writes events to the NewRelic Insights Sink
        /// </summary>
        /// <param name="loggerConfiguration"></param>
        /// <param name="configurationOptions">Configuration parameters for the NewRelic sink</param>
        /// <param name="formatProvider"></param>
        /// <param name="restrictedToMinimumLevel"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static LoggerConfiguration NewRelicInsights(
            this LoggerSinkConfiguration loggerConfiguration,
            NewRelicConfigurationOptions configurationOptions,
            IFormatProvider formatProvider = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException(nameof(loggerConfiguration));
            
            return loggerConfiguration.Sink(
                new NewRelicInsightsSink(formatProvider, configurationOptions), 
                restrictedToMinimumLevel);
        }

        /// <summary>
        /// Writes events to the NewRelic Insights Sink. Enables configuration through appSettings or other configuration sources
        /// </summary>
        /// <param name="loggerSinkConfiguration"></param>
        /// <param name="applicationName"></param>
        /// <param name="environmentName"></param>
        /// <param name="accountId"></param>
        /// <param name="licenseKey"></param>
        /// <param name="newRelicBaseUri"></param>
        /// <param name="formatProvider"></param>
        /// <param name="restrictedToMinimumLevel"></param>
        /// <returns></returns>
        public static LoggerConfiguration NewRelicInsights(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string applicationName,
            string environmentName,
            string accountId,
            string licenseKey,
            string newRelicBaseUri = "https://insights-collector.newrelic.com/v1/accounts/{0}/events",
            IFormatProvider formatProvider = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (loggerSinkConfiguration == null) throw new ArgumentNullException(nameof(loggerSinkConfiguration));

            var configurationOptions = new NewRelicConfigurationOptions
            {
                AccountId = accountId,
                ApplicationName = applicationName,
                EnvironmentName = environmentName,
                LicenseKey = licenseKey,
                NewRelicBaseUri = newRelicBaseUri
            };

            return loggerSinkConfiguration.Sink(
                new NewRelicInsightsSink(formatProvider, configurationOptions),
                restrictedToMinimumLevel);
        }
    }
}