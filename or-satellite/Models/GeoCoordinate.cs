using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace or_satellite.Models
{
    class GeoCoordinate
    {
        public double Latitude;
        public double Longitude;

        public GeoCoordinate(double _latitude, double _longitude)
        {
            Latitude = _latitude;
            Longitude = _longitude;
        }
    }
}
