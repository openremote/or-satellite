using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using or_satellite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace or_satellite.Service
{
    public class CloudCoverageService
    {
        private string longitude;
        private string latitude;
        private string apiKey = "652042053c32ee4960b68118bbef9166";
        private Int32 endOfDay;
        private Int32 startOfDay;
        private int scanDateTimeEpoch;

        public CloudCoverageService(DateTime date, string ilatitude, string ilongitude, DateTime scanDateTime)
        {

            latitude = ilatitude;
            longitude = ilongitude;
            endOfDay = (Int32)((date.Date + new TimeSpan(23,59,59)).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            startOfDay = (Int32)(date.Date.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            scanDateTimeEpoch = (Int32)((scanDateTime + new TimeSpan(23, 59, 59)).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public async Task<JObject> makeRequest()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync($"https://api.openweathermap.org/data/2.5/onecall/timemachine?lat={latitude}&lon={longitude}&dt={startOfDay}&appid={apiKey}");
            string stringresult = await response.Content.ReadAsStringAsync();
            OpenWeatherMapModel.Rootobject OWMM = new OpenWeatherMapModel.Rootobject();
            OWMM = JsonConvert.DeserializeObject<OpenWeatherMapModel.Rootobject>(stringresult);

            OpenWeatherMapModel.Hourly closestMeasurement = new OpenWeatherMapModel.Hourly();
            long min = long.MaxValue;

            foreach (var item in OWMM.hourly)
                if (Math.Abs(Convert.ToDateTime(item.dt).Ticks - Convert.ToDateTime(scanDateTimeEpoch).Ticks) < min)
                {
                    min = Math.Abs(Convert.ToDateTime(item.dt).Ticks - Convert.ToDateTime(scanDateTimeEpoch).Ticks);
                    closestMeasurement = item;
                }

            int result = closestMeasurement.clouds;
            return JObject.Parse(result.ToString());
        }
    }
}
