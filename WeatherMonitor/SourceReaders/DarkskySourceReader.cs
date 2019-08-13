using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace WeatherMonitor.SourceReaders
{
    using Models;
    using Interfaces;
    using Options;

    public class DarkskySourceReader : ISourceReader
    {
        private const string URL = "https://api.darksky.net/forecast/afb07fd1640071767e85dd1c42de6cc8/";
        private const string DEFAULT_URL = "https://api.darksky.net/forecast/afb07fd1640071767e85dd1c42de6cc8/43.000351,-75.499901?lang=en&units=us&exclude=currently,minutely,daily,alerts,flags";

        private readonly ILogger logger;

        private readonly MonitorOption monitorOption;
        private readonly CityListReader cityListReader;

        private List<DarkskyForecast> forecast;
        private HttpClient client;

        public DarkskySourceReader(ILogger<DarkskySourceReader> logger, MonitorOption monitorOption, CityListReader cityListReader)
        {
            this.logger = logger;
            this.monitorOption = monitorOption;
            this.cityListReader = cityListReader;

            forecast = new List<DarkskyForecast>();
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
                this.logger.LogError(ex, "Darksky catch exception");
                return false;
            }

            if (string.IsNullOrEmpty(result))
            {
                this.logger.LogError("Cannot get info from Darksky");
                return false;
            }
            else
            {
                this.logger.LogInformation("Result is OK");
            }

            var jsonObj = JObject.Parse(result);
            var hourly = jsonObj.GetValue("hourly");
            var data = hourly["data"];
            if(data != null)
            {
                this.forecast = data.ToObject<List<DarkskyForecast>>();
            }
            if (this.forecast.Count > 0)
            {
                DateTime lastDay = this.forecast.First().Time.AddDays(this.monitorOption.Days).Date;

                // select only that days, that we need
                this.forecast = this.forecast.Where(f => f.Time < lastDay).ToList();
                return true;
            }
            else
            {
                this.logger.LogError("Darksky cannot parse forecast");
                return false;
            }

        }
        private string UriBuilder()
        {
            StringBuilder url = new StringBuilder(URL);
            if (this.monitorOption == null) return DEFAULT_URL;

            var city = cityListReader.Cities.Where(c => string.Equals(c.Name, this.monitorOption.City)).FirstOrDefault();
            if(city == null)
            {
                this.logger.LogError($"City '{this.monitorOption.City}' isn't found");
                return DEFAULT_URL;
            }
            else
            {
                url = url.Append(city.Coordinates.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Append(",")
                    .Append(city.Coordinates.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }


            url = url.Append("?lang=").Append(this.monitorOption.Language.ToString().ToLower())
                .Append("&units=");
            
            switch (monitorOption.Unit)
            {
                case Unit.imperial:
                    {
                        url = url.Append("us");
                        break;
                    }
                case Unit.metric:
                    {
                        url = url.Append("si");
                        break;
                    }
                default:
                    {
                        url = url.Append("us");
                        break;
                    }
            }

            url = url.Append("&exclude=currently,minutely,daily,alerts,flags");

            return url.ToString();

        }
        public double GetAveragePresure()
        {
            return this.forecast.Average(f => f.Pressure);
        }

        public Dictionary<DateTime, double> GetAverageTemperature()
        {
            Dictionary<DateTime,double> temperatureByDay = new Dictionary<DateTime, double>();
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
                    this.logger.LogError($"Darksky: {startDay.ToShortDateString()} - {endDay.ToShortDateString()} doesn't have info about temperatura");
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
                currentIndex = this.forecast.FindIndex(currentIndex, f =>f.RainProbility > 0.5);
                if (currentIndex != -1)
                {
                    RainTimeSpan span = new RainTimeSpan
                    {
                        Start = this.forecast[currentIndex].Time
                    };

                    currentIndex = this.forecast.FindIndex(currentIndex, f =>f.RainProbility < 0.5);
                    if (currentIndex == -1)
                    {
                        span.End = this.forecast.Last().Time.AddHours(1);
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
