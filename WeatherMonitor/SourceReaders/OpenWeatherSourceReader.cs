using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;

namespace WeatherMonitor.SourceReaders
{
    using Interfaces;
    using Models;
    using Options;

    public class OpenWeatherSourceReader : ISourceReader
    {
        private const string URL = "https://api.openweathermap.org/data/2.5/forecast?appid=77bea68862c21b7c9eb039c704002d81";
        private const string DEFAULT_URL = "https://api.openweathermap.org/data/2.5/forecast?appid=77bea68862c21b7c9eb039c704002d81&q=New York,us&units=imperial&lang=en_us";

        private readonly ILogger logger;

        private readonly MonitorOption monitorOption;
        private List<OpenWeatherForecast> forecast;
        private HttpClient client;

        public OpenWeatherSourceReader(ILogger<OpenWeatherSourceReader> logger, MonitorOption monitorOption)
        {
            this.logger = logger;
            this.monitorOption = monitorOption;

            forecast = new List<OpenWeatherForecast>();
            client = new HttpClient();

        }
        public async Task<bool> ReadForecast()
        {
            var uri = UriBuilder();
            this.logger.LogInformation("Sent request " + uri);

            string result;
            try
            {
                result = await client.GetStringAsync(uri);
            }
            catch(Exception ex)
            {
                this.logger.LogError(ex, "OpenWeather catch exception");
                return false;
            }

            if (string.IsNullOrEmpty(result))
            {
                this.logger.LogError("Cannot get info from OpenWeatherMap.org");
                return false;
            }
            else
            {
                this.logger.LogInformation("Result is OK");
            }

            var jsonObj = JObject.Parse(result);
            var list = jsonObj.GetValue("list");
            this.forecast = list.ToObject<List<OpenWeatherForecast>>();
            if (this.forecast.Count > 0)
            {
                DateTime lastDay = this.forecast.First().Time.AddDays(this.monitorOption.Days).Date;

                // select only that days, that we need
                this.forecast = this.forecast.Where(f => f.Time < lastDay).ToList();

                return true;
            }
            else
            {
                this.logger.LogError("Parsing weather forecast return Zero element");
                return false;
            }

        }
        private string UriBuilder()
        {
            StringBuilder url = new StringBuilder(URL);
            if (monitorOption == null) return DEFAULT_URL;

            url = url.Append("&q=").Append(monitorOption.City).Append(",").Append(monitorOption.Country.ToLower())
                .Append("&units=").Append(monitorOption.Unit.ToString().ToLower())
                .Append("&lang=");
            #region Language switch case
            switch (monitorOption.Language)
            {
                case Language.en:
                    {
                        url = url.Append("en_us");
                        break;
                    }
                case Language.sv:
                    {
                        url = url.Append("sv_se");
                        break;
                    }
                case Language.zh:
                    {
                        url = url.Append("zh_cn");
                        break;
                    }
                case Language.ru:
                    {
                        url = url.Append("ru_ru");
                        break;
                    }
                default:
                    {
                        url = url.Append("en_us");
                        break;
                    }

            }
            #endregion

            return url.ToString();

        }

        public double GetAveragePresure()
        {
            return this.forecast.Average(f => f.Pressure);
        }

        public Dictionary<DateTime, double> GetAverageTemperature()
        {
            Dictionary<DateTime, double> temperatureByDay = new Dictionary<DateTime, double>();
            var startDay = this.forecast.First().Time.Date;
            var endDay = startDay.AddDays(1).Date;


            for (int i = 0; i < this.monitorOption.Days; i++)
            {
                var dayForecast = this.forecast
                    .SkipWhile(f => f.Time < startDay)
                    .TakeWhile(f => f.Time < endDay)
                    .ToList();
                if (dayForecast.Count == 0)
                {
                    this.logger.LogError($"{startDay.ToShortDateString()} - {endDay.ToShortDateString()} has Zero element of forecast");
                }
                else
                {
                    var temp = dayForecast.Average(f => f.Temperature);
                    temperatureByDay.Add(startDay, temp);
                }

                startDay = startDay.AddDays(1).Date;
                endDay = endDay.AddDays(1).Date;
            }

            return temperatureByDay;
        }

        public List<RainTimeSpan> GetRainInfo()
        {
            List<RainTimeSpan> rainTimeSpans = new List<RainTimeSpan>();
            int currentIndex = 0;

            while (currentIndex < this.forecast.Count && currentIndex != -1)
            {
                currentIndex = this.forecast.FindIndex(currentIndex, f => f.Rain != null);
                if (currentIndex != -1)
                {
                    RainTimeSpan span = new RainTimeSpan
                    {
                        Start = this.forecast[currentIndex].Time
                    };

                    currentIndex = this.forecast.FindIndex(currentIndex, f => f.Rain == null);
                    if (currentIndex == -1)
                    {
                        span.End = this.forecast.Last().Time.AddHours(3);
                    }
                    else
                    {
                        span.End = this.forecast[currentIndex].Time;
                    }

                    rainTimeSpans.Add(span);
                }
            }

            return rainTimeSpans;
        }
    }
}
