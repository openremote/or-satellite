using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CopernicusOpenCSharp;
using System.Threading.Tasks;
using CopernicusOpenCSharp.Extensions;
using CopernicusOpenCSharp.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Routing.Constraints;

namespace or_satellite.Service
{
    public class CopernicusGetData
    {
        private HttpClient _client = new HttpClient();
        private string username = "openremote";
        private string password = "cOs81$vZ^1Wj";
        
        public async Task<QueryId> GetDataAsync(double longitude, double latitude)
        {
            CopernicusService service = new CopernicusService("openremote", "cOs81$vZ^1Wj");

            string id = "'571e31dc-cd20-48c3-b61b-0802f032aa53'";
            var test = await service.GetDataAsync(id: id);
            var test2 = test.ExtractJsonId();

            return test2;
        }

        static Progress<double> myProgress = new Progress<double>();
        private static double old = 0;

        public async Task Test()
        {
            myProgress.ProgressChanged += (sender, value) =>
            {
                value = Math.Round(value, 2);

                if (value != old)
                {
                    Console.WriteLine($"{value.ToString("#.##")} %");
                }

                old = value;
            };
            CopernicusService service = new CopernicusService("openremote", "cOs81$vZ^1Wj");
            string id = "'571e31dc-cd20-48c3-b61b-0802f032aa53'";

            var download = await service.DownloadMetaDataAsync("/app/copernicus", myProgress, id: id);
        }

        public async Task<string> GetId(double longitude, double latitude)
        {
            string endOfDay = DateTime.UtcNow.ToString("yyyy-MM-ddT23:59:59.999Z");
            string startOfDay = DateTime.UtcNow.ToString("yyyy-MM-ddT00:00:00.000Z");

            string requestUri = $"https://scihub.copernicus.eu/dhus/search?q=( footprint:\"Intersects({longitude}, {latitude})\") AND ( beginPosition:[{startOfDay} TO {endOfDay}] AND endPosition:[{startOfDay} TO {endOfDay}] ) AND ( (platformname:Sentinel-3 AND filename:S3A_* AND producttype:OL_2_LFR___ AND instrumentshortname:OLCI AND productlevel:L2))";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes(
                    $"{username}:{password}")));
            var result = await _client.GetAsync(requestUri);

            result.EnsureSuccessStatusCode();
            var response = await result.Content.ReadAsAsync<Feed>();
            
            //TODO: HTTP CLIENT
            //TODO: request met url

            return response.Id;
        }


    }
}