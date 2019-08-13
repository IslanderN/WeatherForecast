using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace WeatherMonitor.Models
{
    public class OpenWeatherForecast
    {
        [JsonProperty("dt")]
        [JsonConverter(typeof(MicrosecondEpochConverter))]
        public DateTime Time { get; set; }

        public double Temperature { get; set; }

        public double Pressure { get; set; }

        [JsonProperty("rain")]
        public IDictionary<string, JToken> Rain { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> additionalData;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            var mainToken = additionalData["main"];
            if (mainToken != null)
            {
                this.Temperature= mainToken.Value<double>("temp");
                this.Pressure = mainToken.Value<double>("pressure");
            }

        }
        public OpenWeatherForecast()
        {
            additionalData = new Dictionary<string, JToken>();
        }
    }

    public class MicrosecondEpochConverter : DateTimeConverterBase
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((DateTime)value - _epoch).TotalSeconds.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) { return null; }
            return _epoch.AddSeconds((long)reader.Value );
        }
    }

}
