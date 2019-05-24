using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.NewRelic;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configure via environment variables
            var configBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables();
            var configuration = configBuilder.Build();

            
            Serilog.Debugging.SelfLog.Enable(Console.Out);
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            logger.Information("I said hello at time {DateTime}, {CorrelationId}", DateTime.UtcNow, Guid.NewGuid());

            // In a dotnet core application that uses hosting, we would wire up Serilog with 
            // Dispose to ensure that the log queue is flushed
            // This is required so we flush events out before the application closes
            logger.Dispose();
        }

        /*
         * Alternately, you can configure via code:
         *
         *
           var newRelicConfigurationOptions = new NewRelicConfigurationOptions
               {
                   AccountId = "<your-account-id>",
                   ApplicationName = "ConsoleApp",
                   EnvironmentName = "Development",
                   LicenseKey = "your-license-key"
               };
           var logger = new LoggerConfiguration()
               .Enrich.FromLogContext()
               .WriteTo.NewRelicInsights(
                   newRelicConfigurationOptions, 
                   restrictedToMinimumLevel:LogEventLevel.Information)
               .ReadFrom.Configuration(configuration)
               .CreateLogger();
         */
    }
}