using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using or_satellite.Service;
using System.Threading.Tasks;
using or_satellite.Models;

namespace or_satellite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CopernicusController : Controller
    {
        private CopernicusGetData copernicus = new CopernicusGetData();

        [HttpGet("getid")]
        public async Task<IEnumerable<string>> GetId(double longitude, double latitude)
        {
            var output = await copernicus.GetId(longitude, latitude);

            return output;
        }
    }
}