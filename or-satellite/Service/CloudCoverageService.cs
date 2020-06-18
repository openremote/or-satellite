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
        private string apiKey = "e7531acbe7d78f82b7352a46e9e8890a";
        private Int32 endOfDay;
        private Int32 startOfDay;

        public CloudCoverageService(DateTime date, string ilatitude, string ilongitude)
        {

            latitude = ilatitude;
            longitude = ilongitude;
            endOfDay = (Int32)((date.Date + new TimeSpan(23,59,59)).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            startOfDay = (Int32)(date.Date.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public async Task<JObject> makeRequest()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync($"http://history.openweathermap.org/data/2.5/history/city?lat={latitude}&lon={longitude}&type=hour&start={startOfDay}&end={endOfDay}&appid={apiKey}");
            string stringresult = await response.Content.ReadAsStringAsync();
            OpenWeatherMapModel.Clouds OWMM = new OpenWeatherMapModel.Clouds();
            OWMM = JsonConvert.DeserializeObject<OpenWeatherMapModel.Clouds>(stringresult);
            int result = OWMM.all;
            return JObject.Parse(result.ToString());
        }
    }
}
