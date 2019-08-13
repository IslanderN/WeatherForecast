using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace WeatherMonitor.Models
{
    public class DarkskyForecast
    {
        [JsonProperty("time")]
        [JsonConverter(typeof(MicrosecondEpochConverter))]
        public DateTime Time { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }

        [JsonProperty("pressure")]
        public double Pressure { get; set; }

        [JsonProperty("precipIntensity")]
        public double RainIntensity { get; set; }

        [JsonProperty("precipProbability")]
        public double RainProbility { get; set; }
    }
}
