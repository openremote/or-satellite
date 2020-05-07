using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private string username;
        private string password;

        static Progress<double> myProgress = new Progress<double>();
        private static double old = 0;

        public CopernicusGetData(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public bool CheckIfDirectoryExists(DateTime date)
        {
            string directory = $"/app/Copernicus/Extraction/{date.ToString("MM-dd-yyyy")}";

            if (Directory.Exists(directory))
                return true;

            return false;
        }

        public async Task<IEnumerable<string>> GetId(double longitude, double latitude, string date)
        {
            Console.WriteLine("Check directories");
            CheckDirectories();
            Console.WriteLine("Searching file...");
            string endOfDay = $"{date}T23:59:59.999Z";
            string startOfDay = $"{date}T00:00:00.000Z";

            string requestUri = $"https://scihub.copernicus.eu/dhus/search?q=( footprint:\"Intersects({longitude}, {latitude})\" ) AND ( beginPosition:[{startOfDay} TO {endOfDay}] AND endPosition:[{startOfDay} TO {endOfDay}] ) AND ( (platformname:Sentinel-3 AND filename:S3A_* AND producttype:OL_2_LFR___ AND instrumentshortname:OLCI AND productlevel:L2))&sortedby=ingestiondate&order=desc";
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
            {
                Console.WriteLine("No matches found");
                return new[] { finalMessage.totalResults + " results" };
            }
                
            Console.WriteLine("File has been found");
            await DownloadMapData($"'{finalMessage.entry[0].id}'", finalMessage.entry[0].title, finalMessage.entry[0].date[1].Value);

            return finalMessage.entry.Select(s => s.id);
        }

        private async Task DownloadMapData(string id, string title, DateTime ingestionDate)
        {
            Console.WriteLine("Downloading file");
            myProgress.ProgressChanged += (sender, value) =>
            {
                value = Math.Round(value);

                if (value == old)
                {
                    Console.WriteLine(value + "% downloaded ");
                    old = value + 10;
                }
            };
            CopernicusService service = new CopernicusService(username, password);

            await service.DownloadMetaDataAsync("/app/Copernicus/Downloads", myProgress, id: id);
            Console.WriteLine("File downloaded");
            ExtractData(title, ingestionDate);
        }

        private void ExtractData(string title, DateTime startDate)
        {
            Console.WriteLine("Extracting file");
            string startPath = "/app/Copernicus/Downloads";
            string zipPath = $"{title}.zip";
            string extractPath = "/app/Copernicus/Extraction";
            string date = startDate.ToString("dd-MM-yyyy");
            if(Directory.Exists($"{extractPath}/{date}"))
                Directory.Delete($"{extractPath}/{date}", true);
            Directory.CreateDirectory($"{extractPath}/{date}");

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
            Console.WriteLine("File extracted");
            ProcessData(startDate);
        }

        public void ProcessData(DateTime startDate)
        {
            string startPath = "/app/Copernicus/Processed";
            string date = startDate.ToString("dd-MM-yyyy");
            string extractedFilesPath = $"/app/Copernicus/Extraction/{date}";
            string mergeAtmosphericAndCoord = $"ncks -A {extractedFilesPath}/tie_meteo.nc {extractedFilesPath}/tie_geo_coordinates.nc";
            string dimensionSplitting =
                $"ncks -d tie_pressure_levels,0,0 {extractedFilesPath}/tie_geo_coordinates.nc {extractedFilesPath}/filtered_merged_file.nc";
            string extractLatitude = $"ncks -s '%i\n' -F -H -C -v latitude {extractedFilesPath}/filtered_merged_file.nc > {startPath}/{date}/latitudes.txt";
            string extractLongitude =
                $"ncks -s '%i\n' -F -H -C -v longitude {extractedFilesPath}/filtered_merged_file.nc > {startPath}/{date}/longitudes.txt";
            string mergeCoordinates = $"paste -d , {startPath}/{date}/longitudes.txt {startPath}/{date}/latitudes.txt > {startPath}/{date}/coordfile.txt";
            string temperature =
                $"ncks -s '%.3f\n' -F -H -C -v atmospheric_temperature_profile {extractedFilesPath}/filtered_merged_file.nc > {startPath}/{date}/temperature.txt";
            string humidity =
                $"ncks -s '%f\n' -F -H -C -v humidity {extractedFilesPath}/filtered_merged_file.nc > {startPath}/{date}/humidity.txt";
            string seaPressure =
                $"ncks -s '%f\n' -F -H -C -v sea_level_pressure {extractedFilesPath}/filtered_merged_file.nc > {startPath}/{date}/sea_pressure.txt";
            string totalOzone =
                $"ncks -s '%f\n' -F -H -C -v total_ozone {extractedFilesPath}/filtered_merged_file.nc > {startPath}/{date}/total_ozone.txt";

            Console.WriteLine("Processing files");
            if (Directory.Exists($"{startDate}/{date}"))
                Directory.Delete($"{startDate}/{date}",true);
            Directory.CreateDirectory($"{startPath}/{date}");

            Console.WriteLine(mergeAtmosphericAndCoord);
            ExecuteCommand(mergeAtmosphericAndCoord);
            Console.WriteLine(dimensionSplitting);
            ExecuteCommand(dimensionSplitting);
            Console.WriteLine(extractLatitude);
            ExecuteCommand(extractLatitude);
            Console.WriteLine(extractLongitude);
            ExecuteCommand(extractLongitude);
            Console.WriteLine(mergeCoordinates);
            ExecuteCommand(mergeCoordinates);
            Console.WriteLine(temperature);
            ExecuteCommand(temperature);
            Console.WriteLine(humidity);
            ExecuteCommand(humidity);
            Console.WriteLine(seaPressure);
            ExecuteCommand(seaPressure);
            Console.WriteLine(totalOzone);
            ExecuteCommand(totalOzone);
            Console.WriteLine("Files has been processed");
        }

        private void ExecuteCommand(string command)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private IEnumerable<string> ExecuteCommandWithOutput(string command)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string readConsole = process.StandardOutput.ReadToEnd().Replace("n", "\n");
            IEnumerable<string> output = SetResultToArray(readConsole);
            process.WaitForExit();

            return output;
        }

        public IEnumerable<string> GetLocationInfo(string longitude, string latitude, DateTime date)
        {
            string command = $"./LocSearchCore -coords {latitude} {longitude} {date.ToString("dd-MM-yyyy")}/";
            IEnumerable<string> output = ExecuteCommandWithOutput(command);

            return output;
        }

        private IEnumerable<string> SetResultToArray(string input)
        {
            IEnumerable<string> result = input.Split("\n");
            return result;
        }

        private void CheckDirectories()
        {
            if (!Directory.Exists("/app/Copernicus"))
                Directory.CreateDirectory("/app/Copernicus");
            if (!Directory.Exists("/app/Copernicus/Downloads"))
                Directory.CreateDirectory("/app/Copernicus/Downloads");
            if (!Directory.Exists("/app/Copernicus/Extraction"))
                Directory.CreateDirectory("/app/Copernicus/Extraction");
            if (!Directory.Exists("/app/Copernicus/Processed"))
                Directory.CreateDirectory("/app/Copernicus/Processed");
        }



    }
}