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
        public async Task<string> GetValues(string longitude, string latitude, DateTime? date = null)
        {
            string result = "the request could not be handled";

            if (!date.HasValue)
            {
                date = DateTime.Now;
            }
                
            

            SearchResultModel output = locSearch.Search(latitude, longitude, Convert.ToDateTime(date));

            switch (output.searchResult)
            {
                case SearchResultEnum.dataNotAvialable:
                    await copernicus.GetId(Convert.ToDouble(latitude), Convert.ToDouble(longitude), Convert.ToDateTime(date));
                    result = locSearch.execute(output);
                    break;
                case SearchResultEnum.missingFiles:
                    await copernicus.GetId(Convert.ToDouble(latitude), Convert.ToDouble(longitude), Convert.ToDateTime(date));
                    result = locSearch.execute(output);
                    break;
                case SearchResultEnum.success:
                    result = locSearch.execute(output);
                    break;
            }
            return result;
        }
    }
}