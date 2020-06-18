using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace or_satellite.Models
{
    public class OpenWeatherMapModel
    {
        public class Rootobject
        {
            public string message { get; set; }
            public string cod { get; set; }
            public int city_id { get; set; }
            public float calctime { get; set; }
            public int cnt { get; set; }
            public List[] list { get; set; }
        }

        public class List
        {
            public Main main { get; set; }
            public Wind wind { get; set; }
            public Clouds clouds { get; set; }
            public Weather[] weather { get; set; }
            public int dt { get; set; }
        }

        public class Main
        {
            public float temp { get; set; }
            public float temp_min { get; set; }
            public float temp_max { get; set; }
            public float pressure { get; set; }
            public float sea_level { get; set; }
            public float grnd_level { get; set; }
            public int humidity { get; set; }
        }

        public class Wind
        {
            public float speed { get; set; }
            public float deg { get; set; }
        }

        public class Clouds
        {
            public int all { get; set; }
        }

        public class Weather
        {
            public int id { get; set; }
            public string main { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }

    }
}
