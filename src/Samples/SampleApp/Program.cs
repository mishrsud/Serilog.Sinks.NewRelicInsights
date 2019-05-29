using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.NewRelicInsights;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Serilog.Debugging.SelfLog.Enable(Console.Out);
            Log.Logger = CreateLoggerWithEnvironmentVariableConfiguration();

            Log.Information("I said hello at time {DateTime}, {CorrelationId}", DateTime.UtcNow, Guid.NewGuid());
            
            // This is required so we flush events out before the application closes
            Log.CloseAndFlush();
        }

        /// <summary>
        /// Configures a <see cref="Logger"/> with configuration read from environment variables
        /// </summary>
        private static Logger CreateLoggerWithEnvironmentVariableConfiguration()
        {
            // Configure via environment variables
            var configBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables();
            var configuration = configBuilder.Build();
            
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            return logger;
        }

        /// <summary>
        /// Configures a logger using configuration through the fluent API
        /// </summary>
        private static Logger CreateLoggerWithCodeConfiguration()
        {
            /*
             * Alternately, you can configure via code:
             *
             */
           var newRelicConfigurationOptions = new NewRelicConfigurationOptions
               {
                   AccountId = "<your-account-key>",
                   ApplicationName = "ConsoleApp",
                   EnvironmentName = "Development",
                   EventType = "MyEvent",
                   LicenseKey = "your-license-key"
               };
           var logger = new LoggerConfiguration()
               .Enrich.FromLogContext()
               .WriteTo.NewRelicInsights(
                   newRelicConfigurationOptions, 
                   restrictedToMinimumLevel:LogEventLevel.Information)
               .CreateLogger();
           return logger;
        }
        
    }
}