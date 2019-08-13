using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherMonitor.Interfaces
{
    public interface ISourceReader
    {
        Task<bool> ReadForecast();
        Dictionary<DateTime, double> GetAverageTemperature();

        double GetAveragePresure();
        List<RainTimeSpan> GetRainInfo();
    }

    public class RainTimeSpan
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
