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
        private readonly CopernicusGetData copernicus;
        private readonly LocationSearch locSearch;

        public CopernicusController(CopernicusGetData copernicus, LocationSearch locSearch)
        {
            this.copernicus = copernicus;
            this.locSearch = locSearch;
        }

        [HttpGet("getid")]
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