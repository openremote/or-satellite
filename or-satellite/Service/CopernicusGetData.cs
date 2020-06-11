using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using CopernicusOpenCSharp;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CopernicusOpenCSharp.Extensions;
using CopernicusOpenCSharp.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Logging;
using or_satellite.Models;

namespace or_satellite.Service
{
    public class CopernicusGetData
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string username;
        private readonly string password;
        private DatasetFootprintCheck geoCalculator = new DatasetFootprintCheck();

        private static readonly Progress<double> MyProgress = new Progress<double>();
        private static double old = 0;
        // private readonly ILogger logger;

        /*public CopernicusGetData(string username, string password, ILogger logger)
        {
            this.username = username;
            this.password = password;-
            this.logger = logger;
        }*/

        public CopernicusGetData(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public async Task<SearchResultModel> GetId(double latitude, double longitude, DateTime date)
        {
            // logger.LogInformation("Checking directories...");
            CheckBaseDirectories();
            
            // logger.LogInformation("Searching file...");
            string endOfDay = $"{date:yyyy-MM-dd}T23:59:59.999Z";
            string startOfDay = $"{date:yyyy-MM-dd}T00:00:00.000Z";

            string requestUri = $"https://scihub.copernicus.eu/dhus/search?q=( footprint:\"Intersects({latitude}, {longitude})\" ) AND " +
                                $"( beginPosition:[{startOfDay} TO {endOfDay}] AND " +
                                $"endPosition:[{startOfDay} TO {endOfDay}] ) AND " +
                                "( (platformname:Sentinel-3 AND filename:S3A_* AND producttype:OL_2_LFR___ AND instrumentshortname:OLCI AND productlevel:L2))" +
                                "&sortedby=ingestiondate&order=desc";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes(
                    $"{username}:{password}")));
            var result = await client.GetAsync(requestUri);

            result.EnsureSuccessStatusCode();
            var response = await result.Content.ReadAsStringAsync();

            XmlSerializer serializer = new XmlSerializer(typeof(feed));
            StringReader reader = new StringReader(response);
            feed finalMessage = (feed) serializer.Deserialize(reader);

            if (finalMessage.entry == null)
            {
                return new SearchResultModel { latitude = latitude.ToString(), longitude = longitude.ToString(), date = date, searchResult = SearchResultEnum.noDatasetFound};
            }

            /*if (Directory.Exists($"/app/Copernicus/Processed/{date:dd-MM-yyy}/{finalMessage.entry[0].id}"))
                return new SearchResultModel { searchResult = SearchResultEnum.dataAlreadyProcessed };*/

            var metaData = finalMessage.entry[0].str[1].Value;
            Regex pattern = new Regex(@"(?:<gml:coordinates>)(.*.)(?:<)");
            metaData = pattern.Match(metaData).Groups[1].ToString()/*.Replace(" ", ";\n")*/;
            metaData = $"{finalMessage.entry[0].id}:{metaData}";
            Directory.CreateDirectory($"/app/Copernicus/Processed/{date:dd-MM-yyyy}");
            File.AppendAllText($"/app/Copernicus/Processed/{date:dd-MM-yyyy}/metadata.txt", metaData + "\n");

            await DownloadMapData($"'{finalMessage.entry[0].id}'", finalMessage.entry[0].title, finalMessage.entry[0].date[1].Value);

            return new SearchResultModel { latitude = latitude.ToString(), longitude = longitude.ToString(), date = date, searchResult = SearchResultEnum.success };
        }

        private async Task DownloadMapData(string id, string title, DateTime ingestionDate)
        {
            Console.WriteLine("Downloading file");
            MyProgress.ProgressChanged += (sender, value) =>
            {
                value = Math.Round(value);

                if (value == old)
                {
                    Console.WriteLine(value + "% downloaded ");
                    old = value + 10;
                }
            };
            CopernicusService service = new CopernicusService(username, password);

            await service.DownloadMetaDataAsync("/app/Copernicus/Downloads", MyProgress, id: id);
            Console.WriteLine("File downloaded");
            ExtractData(id, title, ingestionDate);
        }

        private void ExtractData(string id, string title, DateTime startDate)
        {
            Console.WriteLine("Extracting file");
            string startPath = "/app/Copernicus/Downloads";
            string zipPath = $"{title}.zip";
            string extractPath = "/app/Copernicus/Extraction";
            string date = startDate.ToString("dd-MM-yyyy");
            if(Directory.Exists($"{extractPath}/{date}/{id.Replace("\'", "")}"))
                Directory.Delete($"{extractPath}/{date}/{id.Replace("\'", "")}", true);
            Directory.CreateDirectory($"{extractPath}/{date}/{id.Replace("\'", "")}");

            using (ZipArchive archive = ZipFile.OpenRead($"{startPath}/{zipPath}"))
            {
                var result = from currEntry in archive.Entries
                    where Path.GetDirectoryName(currEntry.FullName) == title + ".SEN3"
                    where !String.IsNullOrEmpty(currEntry.Name)
                    select currEntry;

                foreach (ZipArchiveEntry entry in result)
                {
                    entry.ExtractToFile(Path.Combine($"{extractPath}/{date}/{id.Replace("\'", "")}", entry.Name));
                }
            }
            Console.WriteLine("File extracted");
            ProcessData(id, startDate);
        }

        public void ProcessData(string id, DateTime startDate)
        {
            
            string date = startDate.ToString("dd-MM-yyyy");
            string startPath = $"/app/Copernicus/Processed/{date}/{id.Replace("\'", "")}";
            string extractedFilesPath = $"/app/Copernicus/Extraction/{date}/{id.Replace("\'", "")}";
            string mergeAtmosphericAndCoord = $"ncks -A {extractedFilesPath}/tie_meteo.nc {extractedFilesPath}/tie_geo_coordinates.nc";
            string dimensionSplitting =
                $"ncks -d tie_pressure_levels,0,0 {extractedFilesPath}/tie_geo_coordinates.nc {extractedFilesPath}/filtered_merged_file.nc";
            string extractLatitude = $"ncks -s '%i\n' -F -H -C -v latitude {extractedFilesPath}/filtered_merged_file.nc > {startPath}/latitudes.txt";
            string extractLongitude =
                $"ncks -s '%i\n' -F -H -C -v longitude {extractedFilesPath}/filtered_merged_file.nc > {startPath}/longitudes.txt";
            string mergeCoordinates = $"paste -d , {startPath}/longitudes.txt {startPath}/latitudes.txt > {startPath}/coordfile.txt";
            string temperature =
                $"ncks -s '%.3f\n' -F -H -C -v atmospheric_temperature_profile {extractedFilesPath}/filtered_merged_file.nc > {startPath}/temperature.txt";
            string humidity =
                $"ncks -s '%f\n' -F -H -C -v humidity {extractedFilesPath}/filtered_merged_file.nc > {startPath}/humidity.txt";
            string seaPressure =
                $"ncks -s '%f\n' -F -H -C -v sea_level_pressure {extractedFilesPath}/filtered_merged_file.nc > {startPath}/sea_pressure.txt";
            string totalOzone =
                $"ncks -s '%f\n' -F -H -C -v total_ozone {extractedFilesPath}/filtered_merged_file.nc > {startPath}/total_ozone.txt";

            Console.WriteLine("Processing files");
            if (Directory.Exists($"{startPath}"))
                Directory.Delete($"{startPath}",true);
            Directory.CreateDirectory($"{startPath}");

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
            string command = $"./LocSearchCore -coords {latitude} {longitude} {date:dd-MM-yyyy}/";
            IEnumerable<string> output = ExecuteCommandWithOutput(command);

            return output;
        }

        private IEnumerable<string> SetResultToArray(string input)
        {
            IEnumerable<string> result = input.Split("\n");
            return result;
        }

        private void CheckBaseDirectories()
        {
            // logger.LogInformation("Checking 'Copernicus' folder");
            if (!Directory.Exists("/app/Copernicus"))
            {
                // logger.LogInformation("Directory not found. Creating 'Copernicus' folder'");
                Directory.CreateDirectory("/app/Copernicus");
            }

            // logger.LogInformation("Checking 'Downloads' folder");
            if (!Directory.Exists("/app/Copernicus/Downloads"))
            {
                // logger.LogInformation("Directory not found. Creating 'Downloads' folder'");
                Directory.CreateDirectory("/app/Copernicus/Downloads");
            }

            // logger.LogInformation("Checking 'Extraction' folder");
            if (!Directory.Exists("/app/Copernicus/Extraction"))
            {
                // logger.LogInformation("Directory not found. Creating 'Extraction' folder'");
                Directory.CreateDirectory("/app/Copernicus/Extraction");
            }

            // logger.LogInformation("Checking 'Processed' folder");
            if (!Directory.Exists("/app/Copernicus/Processed"))
            {
                // logger.LogInformation("Directory not found. Creating 'Processed' folder'");
                Directory.CreateDirectory("/app/Copernicus/Processed");
            }
        }
    }
}