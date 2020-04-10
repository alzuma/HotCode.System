using System;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace HotCode.System.Logging
{
    public static class Extensions
    {
        public static IWebHostBuilder UseLogging(this IWebHostBuilder webHostBuilder, string applicationName = null)
            => webHostBuilder.UseSerilog((context, loggerConfiguration) =>
            {
                var appOptions = context.Configuration.GetOptions<AppOptions>("app");
                var serilogOptions = context.Configuration.GetOptions<SerilogOptions>("serilog");
                if (!Enum.TryParse<LogEventLevel>(serilogOptions.Level, true, out var level))
                {
                    level = LogEventLevel.Information;
                }

                applicationName = string.IsNullOrWhiteSpace(applicationName) ? appOptions.Name : applicationName;
                loggerConfiguration.Enrich.FromLogContext()
                    .MinimumLevel.Is(level)
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("ApplicationName", applicationName);

                loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
                
                if (serilogOptions.ConsoleEnabled)
                {
                    loggerConfiguration.WriteTo.Console();
                }
            });
    }
}