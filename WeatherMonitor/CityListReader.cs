using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherMonitor
{
    using Models;
    using Newtonsoft.Json;
    using System.IO;

    public class CityListReader
    {
        private const string CITY_FILENAME = "city.list.json";

        public List<CityModel> Cities { get; private set; }
        public void Read()
        {
            var streamReader = new StreamReader(CITY_FILENAME);
            JsonReader reader = new JsonTextReader(streamReader);

            JsonSerializer jsonSerializer = new JsonSerializer();
            this.Cities = jsonSerializer.Deserialize<List<CityModel>>(reader);

            streamReader.Close();
            reader.Close();
        }
    }
}
