using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace WeatherMonitor
{
    using Models;
    using SourceReaders;
    using Options;
    using Interfaces;

    public class BackgroundMonitor : BackgroundService
    {
        private readonly ILogger logger;
        private readonly TimeoutOption timeoutOption;
        private readonly IOutput output;
        private readonly IEnumerable<ISourceReader> sourceReaders;
        private readonly IOptionInput input;

        private Timer checkWeather;
        public BackgroundMonitor(ILoggerFactory loggerFactory, IOutput output, IEnumerable<ISourceReader> sourceReaders, TimeoutOption timeoutOption, IOptionInput input)
        {
            this.logger = loggerFactory.CreateLogger<BackgroundMonitor>();
            this.output = output;
            this.sourceReaders = sourceReaders;
            this.timeoutOption = timeoutOption;
            this.input = input;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            input.Input();

            checkWeather = new Timer(
                (obj) => CheckWeather(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(timeoutOption.TimeoutInMinutes)
                );
            await Task.CompletedTask;
        }
        private void CheckWeather()
        {
            List<double> preasures = new List<double>();
            List<Dictionary<DateTime, double>> temperatures = new List<Dictionary<DateTime, double>>();
            List<List<RainTimeSpan>> rains = new List<List<RainTimeSpan>>();

            foreach (var source in this.sourceReaders)
            {
                var result = Task.Run(() => source.ReadForecast()).GetAwaiter().GetResult();
                if (result)
                {
                    try
                    {
                        var presure = source.GetAveragePresure();
                        preasures.Add(presure);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, $"Catch in {source.GetType().Name} while getting average presure");
                    }
                    try
                    {
                        var temp = source.GetAverageTemperature();
                        if (temp.Count > 0)
                        {
                            temperatures.Add(temp);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, $"Catch in {source.GetType().Name} while getting average temperature");
                    }
                    try
                    {
                        var rainSpan = source.GetRainInfo();
                        if (rainSpan.Count > 0)
                        {
                            rains.Add(rainSpan);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, $"Catch in {source.GetType().Name} while getting rain spans");
                    }
                }
            }

            this.output.StartMessage();
            if (preasures.Count > 0)
            {
                this.output.WriteMessage("Average Preasure: " + preasures.First());
            }
            if (temperatures.Count > 0)
            {
                var temp = temperatures.OrderByDescending(t => t.Count).First();

                this.output.WriteMessage("Average Temperature");
                foreach (var t in temp)
                {
                    this.output.WriteMessage($"{t.Key.ToShortDateString()} : {t.Value}");
                }
            }
            if (rains.Count > 0)
            {
                this.output.WriteMessage("Rains");

                var rainList = rains.OrderByDescending(r => r.Count).First();
                foreach (var r in rainList)
                {
                    var startDay = r.Start.Date;
                    var endDay = r.End.Date;

                    if (startDay != endDay && r.End.Hour != 0)
                    {
                        this.output.WriteMessage(RainString(r.Start, startDay.AddHours(24)));
                        this.output.WriteMessage(RainString(endDay, r.End));
                    }
                    else
                    {
                        this.output.WriteMessage(RainString(r.Start, r.End));

                    }
                }

            }

            this.output.EndMessage();
        }

        private string RainString(DateTime start, DateTime end)
        {
            return $"{start.ToString("dd.MM.yyyy")} {start.ToString("HH:mm")} - {end.ToString("HH:mm")}";
        }

    }
}
