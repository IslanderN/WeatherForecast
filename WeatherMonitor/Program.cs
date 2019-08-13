using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WeatherMonitor
{
    using Options;
    using Interfaces;
    using Realization;
    using SourceReaders;
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    //config.

                })
                .ConfigureServices((hostContext, services) =>
                {

                    services.AddSingleton<MonitorOption>();
                    services.AddSingleton<TimeoutOption>();

                    services.AddSingleton<IOutput, ConsoleOutput>();

                    services.AddSingleton<ISourceReader, OpenWeatherSourceReader>();
                    services.AddSingleton<ISourceReader, DarkskySourceReader>();

                    services.AddSingleton<CityListReader>();

                    services.AddSingleton<IOptionInput, OptionInput>();

                    services.AddHostedService<BackgroundMonitor>();
                })
                .ConfigureLogging(logBuilder=>
                {
                    logBuilder.AddConsole(configure => configure.IncludeScopes = true) ;
                    logBuilder.SetMinimumLevel(LogLevel.Trace);
                });
            await host.RunConsoleAsync();
        }

    }
}
