using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using or_satellite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters;
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
            scanDateTimeEpoch = (Int32)(scanDateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public async Task<JObject> makeRequest()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync($"https://api.openweathermap.org/data/2.5/onecall/timemachine?lat={latitude}&lon={longitude}&dt={startOfDay}&appid={apiKey}");
            string stringresult = await response.Content.ReadAsStringAsync();
            OpenWeatherMapModel OWMM = new OpenWeatherMapModel();
            OWMM = JsonConvert.DeserializeObject<OpenWeatherMapModel>(stringresult);

            //find 'Hourly' closest to scan time
            Hourly FoundItem = OWMM.hourly.OrderBy(x => Math.Abs(x.dt - scanDateTimeEpoch)).First();

            int result = FoundItem.clouds;
            string dataQuality = "high";
            if (result > 33)
            {
                dataQuality = "medium";
                if (result > 66)
                {
                    dataQuality = "low";
                }
            }
            return JObject.Parse($"{{'clouds': {result}, 'dataQuality': '{dataQuality}'}}");
        }
    }
}
