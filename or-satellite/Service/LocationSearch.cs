using GeoCoordinatePortable;
using Newtonsoft.Json;
using or_satellite.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GeoCoordinate = GeoCoordinatePortable.GeoCoordinate;

namespace or_satellite.Service
{
    public class LocationSearch
    {
        string folderPath = "";
        const string CoordFile = "coordfile.txt";
        const string TemperatureFile = "temperature.txt";
        const string HumidityFile = "humidity.txt";
        const string SeaPressureFile = "sea_pressure.txt";
        const string OzoneFile = "total_ozone.txt";
        const double KelvinMinusValue = 272.15;
        const double CoordScalingFactor = 0.000001;
        DatasetFootprintCheck geoCalculate = new DatasetFootprintCheck();

        private string MinRangeCalculator(string input)
        {
            const double range = 0.5;
            double _input = Convert.ToDouble(input);
            double _minInput = _input - range;
            return _minInput.ToString().Split('.')[0];
        }

        private string MaxRangeCalculator(string input)
        {
            double range = 0.5;
            double _input = Convert.ToDouble(input);
            double _maxInput = _input + range;
            return _maxInput.ToString().Split('.')[0];
        }

        public SearchResultModel Search(string latitude, string longitude, DateTime date)
        {
            if (!File.Exists($"/app/Copernicus/Processed/{date:dd-MM-yyyy}/metadata.txt"))
                return new SearchResultModel { latitude = latitude, longitude = longitude, date = date, searchResult = SearchResultEnum.dataNotAvialable };//

            var id = FindId(latitude, longitude, date);

            if (id == null)
                return new SearchResultModel { latitude = latitude, longitude = longitude, date = date, searchResult = SearchResultEnum.dataNotAvialable };//
                

            System.Globalization.CultureInfo customCulture =
                (System.Globalization.CultureInfo) System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            folderPath = $"/app/Copernicus/Processed/{date:dd-MM-yyyy}/{id}/";


            decimal lat = 0;
            decimal Long = 0;


            if (decimal.TryParse(latitude, out lat) && decimal.TryParse(longitude, out Long))
            {
            }

            #region CheckFilesExist

            if (File.Exists($"{folderPath}{CoordFile}") && File.Exists($"{folderPath}{TemperatureFile}") &&
                File.Exists($"{folderPath}{HumidityFile}") && File.Exists($"{folderPath}{SeaPressureFile}") &&
                File.Exists($"{folderPath}{OzoneFile}"))
            {
            }
            else
            {
                Console.WriteLine("Invalid folder or missing files.");
                //LocationObject locErr = new LocationObject(0, "", 0, 0, 0, 0, false, stopwatch.Elapsed);
                // return locErr;
                return new SearchResultModel { latitude = latitude, longitude = longitude, date = date, searchResult = SearchResultEnum.missingFiles }; 
            }
            return new SearchResultModel { latitude = latitude, longitude = longitude, date = date, succes = true, searchResult = SearchResultEnum.success, id = id};
            #endregion

        }

        private string FindId(string latitude, string longitude, DateTime date)
        {
            string id = null;
            foreach (var line in File.ReadLines($"/app/Copernicus/Processed/{date:dd-MM-yyyy}/metadata.txt"))
            {
                List<string> coordList = new List<string>();
                List<Models.GeoCoordinate> vertices = new List<Models.GeoCoordinate>();
                coordList.AddRange(line.Split(':')[1].Split(' '));

                foreach (var coordinate in coordList)
                {
                    double _latitude = Convert.ToDouble(coordinate.Split(',')[0]);
                    double _longitude = Convert.ToDouble(coordinate.Split(',')[1]);

                    vertices.Add(new Models.GeoCoordinate(_latitude, _longitude));
                }

                Models.GeoCoordinate geoCoordinate =
                    new Models.GeoCoordinate(Convert.ToDouble(latitude), Convert.ToDouble(longitude));
                if (geoCalculate.Contains(geoCoordinate, vertices))
                {
                    id = line.Split(":")[0];
                    break;
                }
            }

            return id;
        }

        public string execute(SearchResultModel resultModel)
        {
            string maxLatRange = MaxRangeCalculator(resultModel.latitude);
            string maxLongRange = MaxRangeCalculator(resultModel.longitude);
            string minLatRange = MinRangeCalculator(resultModel.latitude);
            string minLongRange = MinRangeCalculator(resultModel.longitude);

            var id = FindId(resultModel.latitude, resultModel.longitude, resultModel.date);

            folderPath = $"/app/Copernicus/Processed/{resultModel.date:dd-MM-yyyy}/{id}/";

            List<string> listItems = new List<string>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string data = File.ReadAllText($"{folderPath}{CoordFile}");
            listItems.AddRange(data.Split('\n'));

            List<string> FilteredCoordList =
                listItems.Where(x => x.StartsWith(minLongRange) || x.StartsWith(maxLongRange)).ToList();

            FilteredCoordList = FilteredCoordList.Where(x =>
                x.Split(',')[1].StartsWith(minLatRange) || x.Split(',')[1].StartsWith(maxLatRange)).ToList();

            List<GeoCoordinate> coordinateList = new List<GeoCoordinate>();
            foreach (string item in FilteredCoordList)
            {

                double newLat = Convert.ToDouble(item.Split(',')[1]) * CoordScalingFactor;
                double newLong = Convert.ToDouble(item.Split(',')[0]) * CoordScalingFactor;

                //////////////////////////////////////////////////////////
                coordinateList.Add(new GeoCoordinate(newLat, newLong));
                Console.WriteLine($"{newLat},{newLong}");

            }

            if (coordinateList.Count == 0)
            {
                stopwatch.Stop();
                LocationObject locErr = new LocationObject(0, "", 0, 0, 0, 0, false, stopwatch.Elapsed);
                string errorObject = JsonConvert.SerializeObject(locErr);
                Console.WriteLine(errorObject);
                // return locErr;
                return errorObject;
            }

            List<GeoCoordinate> SortedCoordinateListBasedOnDistance = coordinateList.OrderBy(x =>
                x.GetDistanceTo(new GeoCoordinate(Convert.ToDouble(resultModel.latitude), Convert.ToDouble(resultModel.longitude)))).ToList();
            //Console.WriteLine($"{SortedCoordinateListBasedOnDistance[0].Latitude},{SortedCoordinateListBasedOnDistance[0].Longitude}".Replace(".", ""));
            string stringToSearch = $"{SortedCoordinateListBasedOnDistance[0].Latitude / CoordScalingFactor},{SortedCoordinateListBasedOnDistance[0].Longitude / CoordScalingFactor}";

            int FoundCoordIndex = listItems.IndexOf($"{stringToSearch}");
            //temperature
            double FoundTemperature;
            if (double.TryParse(File.ReadLines($"{folderPath}{TemperatureFile}").Skip(FoundCoordIndex).Take(1).First(),
                out FoundTemperature))
            {
                FoundTemperature = FoundTemperature - KelvinMinusValue;
            }

            //Humidity
            double FoundHumidity;
            if (double.TryParse(File.ReadLines($"{folderPath}{HumidityFile}").Skip(FoundCoordIndex).Take(1).First(),
                out FoundHumidity))
            {
                FoundHumidity = Math.Round(FoundHumidity, 1);
            }

            //sea level pressure
            double foundSeaLevelPressure;
            if (double.TryParse(File.ReadLines($"{folderPath}{SeaPressureFile}").Skip(FoundCoordIndex).Take(1).First(),
                out foundSeaLevelPressure))
            {
                foundSeaLevelPressure = Math.Round(foundSeaLevelPressure, 1);
            }

            //Total Ozone
            double foundOzone;
            if (double.TryParse(File.ReadLines($"{folderPath}{OzoneFile}").Skip(FoundCoordIndex).Take(1).First(),
                out foundOzone))
            {
            }

            stopwatch.Stop();

            LocationObject loc = new LocationObject(
                Math.Round(
                    (SortedCoordinateListBasedOnDistance[0]
                         .GetDistanceTo(new GeoCoordinate(Convert.ToDouble(resultModel.latitude), Convert.ToDouble(resultModel.longitude))) /
                     1000), 2),
                $"{SortedCoordinateListBasedOnDistance[0].Latitude},{SortedCoordinateListBasedOnDistance[0].Longitude}",
                Math.Round(FoundTemperature, 1), FoundHumidity, foundSeaLevelPressure, foundOzone, true,
                stopwatch.Elapsed);
            string jsonResult = JsonConvert.SerializeObject(loc);
            // return loc;
            return jsonResult;
        }
    }
}