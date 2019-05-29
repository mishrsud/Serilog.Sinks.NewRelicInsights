[![Build status](https://ci.appveyor.com/api/projects/status/aanoqs7k9swtq7m3?svg=true)](https://ci.appveyor.com/project/mishrsud/serilog-sinks-newrelicinsights)

# Summary
A Serilog Sink that sends log events to NewRelic Insights.

# Usage
```bash
# nuget CLI
nuget install Serilog.Sinks.NewRelicInsights

# dotnet CLI
dotnet add package Serilog.Sinks.NewRelicInsights

```

# Configuration

## Configuration in Code
1. Create an instance of ```NewRelicConfigurationOptions```"
```csharp
var newRelicConfigurationOptions = new NewRelicConfigurationOptions
               {
                   AccountId = "<your-account-key>",
                   ApplicationName = "ConsoleApp",   // Written to NewRelic
                   EnvironmentName = "Development",  // Written to NewRelic
                   EventType = "MyEvent",            // Written to NewRelic
                   LicenseKey = "your-license-key"
               };
```
2. Next, create a logger with the NewRelicInsights Sink using the above configuration:

```csharp
var logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.NewRelicInsights(
        newRelicConfigurationOptions, 
        restrictedToMinimumLevel:LogEventLevel.Information)
    .CreateLogger();
```

3. Finally, set the global logger to this logger and **ensure that you call Serilog.Log.CloseAndFlush()** before application exits.
```csharp
Log.Logger = logger;


// Before the application exits, call CloseAndFlush as the background tasks and log pipelines need to clear
Log.CloseAndFlush();
```

## Configuration using a configuration provider (e.g. Environment Variables / appSettings etc)

1. Set up your configuration source
```json
// Set the following in appSettings.json

{
    "Serilog": {
        "WriteTo": [
            { 
                "Name": "NewRelicInsights",
                "Args": {
                    "applicationName": "MyApp",
                    "environmentName": "Development",
                    "eventType": "MyEvent",
                    "accountId": "your-newrelic-account-id",
                    "licenseKey": "your-license-key"
                }
            }
        ]
    }
}

// OR set the following environment variables: (Windows Syntax shown)

SETX Serilog__WriteTo__0__Name "NewRelicInsights" /M
SETX Serilog__WriteTo__0__Args__applicationName "ConsoleApp" /M
SETX Serilog__WriteTo__0__Args__environmentName "Development" /M
SETX Serilog__WriteTo__0__Args__eventType "MyEvent" /M
SETX Serilog__WriteTo__0__Args__accountId "your-account-id" /M
SETX Serilog__WriteTo__0__Args__licenseKey "your-license-key" /M
```

2. Create a logger
```csharp
// Requires Microsoft.Extensions.Configuration.EnvironmentVariables
var configBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables();

var configuration = configBuilder.Build();

// Requires Serilog and Serilog.Settings.Configuration
var logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
return logger;
```

3. Finally, set the global logger to this logger and **ensure that you call Serilog.Log.CloseAndFlush()** before application exits.
```csharp
Log.Logger = logger;


// Before the application exits, call CloseAndFlush as the background tasks and log pipelines need to clear
Log.CloseAndFlush();
```
