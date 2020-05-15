using System;

namespace or_satellite.Service
{
    public class LocationObject
    {
        public double KMDistance;
        public string location;
        public double temperature;
        public double humidity;
        public double airPressure;
        public double ozone;
        public bool success;
        public TimeSpan timeTaken;
        public LocationObject(double _distance, string _location, double _temperature, double _humidity, double _airPressure, double _ozone, bool _success, TimeSpan _timeTaken)
        {
            KMDistance = _distance;
            location = _location;
            temperature = _temperature;
            humidity = _humidity;
            airPressure = _airPressure;
            ozone = _ozone;
            timeTaken = _timeTaken;
            success = _success;
        }
    }
}