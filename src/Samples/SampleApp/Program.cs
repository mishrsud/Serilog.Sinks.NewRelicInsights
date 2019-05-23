using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.NewRelic;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();

            var httpClientFactory = serviceCollection
                .BuildServiceProvider()
                .GetService<IHttpClientFactory>();
            
            var newRelicConfigurationOptions = new NewRelicConfigurationOptions
            {
                AccountId = "<YOUR_ACCOUNT_ID>",
                ApplicationName = "ConsoleApp",
                EnvironmentName = "Development",
                LicenseKey = "<YOU_KEY>"
            };
            
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.NewRelicInsights(
                    newRelicConfigurationOptions, 
                    httpClientFactory, 
                    restrictedToMinimumLevel:LogEventLevel.Information)
                .CreateLogger();
    
            logger.Information("I said hello at time {DateTime}", DateTime.UtcNow);
        }
    }
}