using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherMonitor.Options
{
    public class MonitorOption
    {
        public int Days { get; set; } = 5;
        public string City { get; set; } = "New York";
        public string Country { get; set; } = "us";
        public Language Language { get; set; } = Language.en;
        public Unit Unit { get; set; } = Unit.imperial;
    }

    public enum Unit
    {
        metric,
        imperial
    }
    public enum Language
    {
        en,
        sv,
        zh,
        ru
    }
}
