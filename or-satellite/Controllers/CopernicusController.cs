using Microsoft.AspNetCore.Mvc;
using or_satellite.Service;
using System.Threading.Tasks;

namespace or_satellite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CopernicusController : Controller
    {
        private CopernicusGetData copernicus = new CopernicusGetData();
   
        [HttpGet]
        public async Task<IActionResult> IndexAsync(double longitude, double latitude)
        {
            var output = await copernicus.GetDataAsync(longitude, latitude);

            return new OkObjectResult(output);
        }
        
        [HttpGet("downloadfile")]
        public async Task DownloadFile()
        {
            await copernicus.Test();
        }

        [HttpGet("getid")]
        public async Task<string> GetId(double longitude, double latitude)
        {
            string output = await copernicus.GetId(longitude, latitude);

            return output;
        }
    }
}