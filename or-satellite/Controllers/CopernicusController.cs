using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using or_satellite.Service;
using System.Threading.Tasks;
using or_satellite.Models;
using System;
using System.Xml;

namespace or_satellite.Controllers
{
#nullable enable
    [ApiController]
    [Route("[controller]")]
    public class CopernicusController : Controller
    {
        private CopernicusGetData copernicus;
        private LocationSearch locSearch;

        public CopernicusController(CopernicusGetData copernicus, LocationSearch locSearch)
        {
            this.copernicus = copernicus;
            this.locSearch = locSearch;
        }

        [HttpGet("getid")]
        public async Task<IEnumerable<string>> GetId(double longitude, double latitude, string? date)
        {
            if (string.IsNullOrEmpty(date))
            {
                date = DateTime.Now.ToString("yyyy-MM-dd");
            }

            if(!copernicus.CheckIfDirectoryExists(Convert.ToDateTime(date)))
            {
                return await copernicus.GetId(longitude, latitude, date);
            }

            return new[] { "this date has already been processed"};
        }

        [HttpGet("process")]
        public void Process(DateTime date)
        {
            copernicus.ProcessData(date);
        }

        [HttpGet("locationInfo")]
        public IEnumerable<string> GetLocationInfo(string longitude, string latitude, DateTime date)
        {
            IEnumerable<string> output = copernicus.GetLocationInfo(longitude, latitude, date);

            return output;
        }

        [HttpGet("getValue")]
        public string GetValues(string longitude, string latitude, string? date)
        {
            if (string.IsNullOrEmpty(date))
            {
                date = DateTime.Now.ToString("yyyy-MM-dd");
            }

            string output = locSearch.Search(latitude, longitude, date);

            return output;
        }
    }
}