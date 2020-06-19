using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace or_satellite.Models
{
        public class OpenWeatherMapModel
        {
            public float lat { get; set; }
            public float lon { get; set; }
            public string timezone { get; set; }
            public int timezone_offset { get; set; }
            public Current current { get; set; }
            public Hourly[] hourly { get; set; }
        }

        public class Current
        {
        }

        public class Hourly
        {
            public int dt { get; set; }
            public float temp { get; set; }
            public float feels_like { get; set; }
            public int pressure { get; set; }
            public int humidity { get; set; }
            public float dew_point { get; set; }
            public int clouds { get; set; }
            public int visibility { get; set; }
            public float wind_speed { get; set; }
            public int wind_deg { get; set; }
            public Weather[] weather { get; set; }
            public Rain rain { get; set; }
        }

        public class Rain
        {
            public float _1h { get; set; }
        }

        public class Weather
        {
            public int id { get; set; }
            public string main { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }
}
