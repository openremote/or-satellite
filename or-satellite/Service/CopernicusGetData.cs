using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CopernicusOpenCSharp;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CopernicusOpenCSharp.Extensions;
using CopernicusOpenCSharp.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Routing.Constraints;
using or_satellite.Models;

namespace or_satellite.Service
{
    public class CopernicusGetData
    {
        private HttpClient _client = new HttpClient();
        private string username = "openremote";
        private string password = "cOs81$vZ^1Wj";

        static Progress<double> myProgress = new Progress<double>();
        private static double old = 0;

        

        public async Task<IEnumerable<string>> GetId(double longitude, double latitude)
        {
            Console.WriteLine("Searching file");
            string endOfDay = DateTime.UtcNow.ToString("yyyy-MM-15T23:59:59.999Z");
            string startOfDay = DateTime.UtcNow.ToString("yyyy-MM-15T00:00:00.000Z");

            string requestUri = $"https://scihub.copernicus.eu/dhus/search?q=( footprint:\"Intersects({longitude}, {latitude})\" ) AND ( beginPosition:[{startOfDay} TO {endOfDay}] AND endPosition:[{startOfDay} TO {endOfDay}] ) AND( ingestionDate:[2020-04-15T00:00:00.000Z TO 2020-04-15T23:59:59.999Z ] ) AND ( (platformname:Sentinel-3 AND filename:S3A_* AND producttype:OL_2_LFR___ AND instrumentshortname:OLCI AND productlevel:L2))&sortedby=ingestiondate&order=desc";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes(
                    $"{username}:{password}")));
            var result = await _client.GetAsync(requestUri);

            result.EnsureSuccessStatusCode();
            var response = await result.Content.ReadAsStringAsync();

            XmlSerializer serializer = new XmlSerializer(typeof(feed));
            StringReader reader = new StringReader(response);
            feed finalMessage = (feed) serializer.Deserialize(reader);

            //TODO: HTTP CLIENT
            //TODO: request met url
            if (finalMessage.entry == null)
                return new[] {finalMessage.totalResults + " results"};

            await DownloadMapData($"'{finalMessage.entry[0].id}'", finalMessage.entry[0].title, finalMessage.entry[0].date[3].Value);

            return finalMessage.entry.Select(s => s.id);
        }

        public async Task DownloadMapData(string id, string title, DateTime ingestionDate)
        {
            Console.WriteLine("Downloading file");
            myProgress.ProgressChanged += (sender, value) =>
            {
                value = Math.Round(value, 2);

                if (value != old)
                {
                    Console.WriteLine(value + " % downloaded ");
                }

                old = value;
            };
            CopernicusService service = new CopernicusService(username, password);

            await service.DownloadMetaDataAsync("/app/Copernicus/Downloads", myProgress, id: id);
            ExtractData(title, ingestionDate);
        }

        public void ExtractData(string title, DateTime ingestionDate)
        {
            Console.WriteLine("Extracting file");
            string startPath = "/app/Copernicus/Downloads";
            string zipPath = $"{title}.zip";
            string extractPath = "/app/Copernicus/Extraction";
            string date = ingestionDate.ToString().Replace("/", "-").Split(' ')[0];

            Directory.CreateDirectory($"{extractPath}/{date}");

            //ZipFile.ExtractToDirectory($"{startPath}/{zipPath}", $"{extractPath}/{date}");

            using (ZipArchive archive = ZipFile.OpenRead($"{startPath}/{zipPath}"))
            {
                var result = from currEntry in archive.Entries
                    where Path.GetDirectoryName(currEntry.FullName) == title + ".SEN3"
                    where !String.IsNullOrEmpty(currEntry.Name)
                    select currEntry;

                foreach (ZipArchiveEntry entry in result)
                {
                    entry.ExtractToFile(Path.Combine($"{extractPath}/{date}", entry.Name));
                }
            }
        }


    }
}