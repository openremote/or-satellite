﻿using System.Collections.Generic;
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

        [HttpGet("getValue")]
        public async Task<ActionResult> GetValues(string longitude, string latitude, string? date)
        {
            DateTime DateTimeDate;
            JObject result = JObject.Parse(@"{ 'Error': 'The request could not be handled.' }");


            if (string.IsNullOrEmpty(date))
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
                    output = await copernicus.GetId(Convert.ToDouble(latitude), Convert.ToDouble(longitude), DateTimeDate);
                    if (output.searchResult == SearchResultEnum.noDatasetFound)
                    {
                        result = JObject.Parse(@"{ 'Error': 'No Dataset Available!' }");
                        break;
                    }
                    result = JObject.Parse(locSearch.execute(output));
                    break;
                case SearchResultEnum.missingFiles:
                    output = await copernicus.GetId(Convert.ToDouble(latitude), Convert.ToDouble(longitude), DateTimeDate);
                    if (output.searchResult == SearchResultEnum.noDatasetFound)
                    {
                        result = JObject.Parse(@"{ 'Error': 'No Dataset Available!' }");
                        break;
                    }
                    result = JObject.Parse(locSearch.execute(output));
                    break;
                case SearchResultEnum.success:
                    result = JObject.Parse(locSearch.execute(output));
                    break;
            }

            if (result.ContainsKey("Error"))
            {
                return Content(result.ToString(), "application/json");
            }

            var ingestionDate = locSearch.FindIngestionDateTime(Convert.ToDateTime(date), output.id);

            JObject cloudCoverage;
            CloudCoverageService ccs = new CloudCoverageService(DateTimeDate, latitude, longitude, Convert.ToDateTime(ingestionDate)/*scandate meegeven*/);
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
            return Content(result.ToString(), "application/json");
        }
    }
}