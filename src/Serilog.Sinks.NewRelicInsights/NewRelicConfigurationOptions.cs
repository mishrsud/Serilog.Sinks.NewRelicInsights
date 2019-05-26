namespace Serilog.Sinks.NewRelicInsights
{
    public class NewRelicConfigurationOptions
    {
        public string ApplicationName { get; set; }
        public string EnvironmentName { get; set; }
        public string AccountId  { get; set; }
        public string LicenseKey  { get; set; }
        public string NewRelicBaseUri { get; set; } = "https://insights-collector.newrelic.com/v1/accounts/{0}/events";
    }
}