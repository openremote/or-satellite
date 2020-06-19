using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using or_satellite.Service;
using System.Threading.Tasks;
using or_satellite.Models;
using System;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace or_satellite.Controllers
{
#nullable enable
    [ApiController]
    [Route("[controller]")]
    public class CopernicusController : Controller
    {
        private readonly CopernicusGetData copernicus;
        private readonly LocationSearch locSearch;

        public CopernicusController(CopernicusGetData copernicus, LocationSearch locSearch)
        {
            this.copernicus = copernicus;
            this.locSearch = locSearch;
        }

        [HttpGet("downloadMapData")]
        public async Task<string> GetId(double latitude, double longitude, DateTime? date = null)
        {
            date ??= DateTime.Now;
            return await copernicus.GetId(latitude, longitude, Convert.ToDateTime(date));
        }

        /*[HttpGet("process")]
        public void Process(DateTime date)
        {
            copernicus.ProcessData(date);
        }*/

        [HttpGet("getValue")]
        public async Task<string> GetValues(string longitude, string latitude, string? date)
        {
            DateTime DateTimeDate;
            JObject result = JObject.Parse(@"{ 'Error': 'The request could not be handled.' }");


            if (!string.IsNullOrEmpty(date))
            {
                DateTimeDate = DateTime.Now;
            }
            else
            {
                DateTimeDate = Convert.ToDateTime(date);
            }

            SearchResultModel output = locSearch.Search(latitude, longitude, DateTimeDate);

            switch (output.searchResult)
            {
                case SearchResultEnum.dataNotAvialable:
                    await copernicus.GetId(Convert.ToDouble(latitude), Convert.ToDouble(longitude), DateTimeDate);
                    result = JObject.Parse(locSearch.execute(output));
                    break;
                case SearchResultEnum.missingFiles:
                    await copernicus.GetId(Convert.ToDouble(latitude), Convert.ToDouble(longitude), DateTimeDate);
                    result = JObject.Parse(locSearch.execute(output));
                    break;
                case SearchResultEnum.success:
                    result = JObject.Parse(locSearch.execute(output));
                    break;
            }

            if (result.ContainsKey("Error"))
            {
                return result.ToString();
            }
            else
            {
                JObject cloudCoverage = new JObject();
                CloudCoverageService ccs = new CloudCoverageService(DateTimeDate, latitude, longitude);
                try
                {
                    cloudCoverage = await ccs.makeRequest();
                }
                catch (Exception)
                {
                    Console.WriteLine("OpenWeatherMap could not be reached :(");
                }
                cloudCoverage = await ccs.makeRequest();
                result.Merge(cloudCoverage);
                return result.ToString();
            }
        }
    }
}