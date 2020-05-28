using or_satellite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace or_satellite.Service
{
    public class DatasetFootprintCheck
    {
        bool Contains(GeoCoordinate location, List<GeoCoordinate> _vertices)
        {
            //Should be from first footprint parameter in the scan metadata
            _vertices.Add(new GeoCoordinate(41.9739, -1.51452));
            _vertices.Add(new GeoCoordinate(41.8965, -0.681643));
            _vertices.Add(new GeoCoordinate(41.8145, 0.140275));
            _vertices.Add(new GeoCoordinate(41.7259, 0.966271));
            _vertices.Add(new GeoCoordinate(41.632, 1.78557));
            _vertices.Add(new GeoCoordinate(41.5326, 2.5995));
            _vertices.Add(new GeoCoordinate(41.427, 3.41511));
            _vertices.Add(new GeoCoordinate(41.3159, 4.22633));
            _vertices.Add(new GeoCoordinate(41.1981, 5.03921));
            _vertices.Add(new GeoCoordinate(41.0757, 5.84244));
            _vertices.Add(new GeoCoordinate(40.9512, 6.65174));
            _vertices.Add(new GeoCoordinate(40.8169, 7.4522));
            _vertices.Add(new GeoCoordinate(40.68, 8.24832));
            _vertices.Add(new GeoCoordinate(40.5319, 9.04305));
            _vertices.Add(new GeoCoordinate(40.3819, 9.83174));
            _vertices.Add(new GeoCoordinate(40.2235, 10.6188));
            _vertices.Add(new GeoCoordinate(40.0629, 11.4053));
            _vertices.Add(new GeoCoordinate(39.8969, 12.1839));
            _vertices.Add(new GeoCoordinate(39.7251, 12.9611));
            _vertices.Add(new GeoCoordinate(39.5488, 13.7311));
            _vertices.Add(new GeoCoordinate(42.1463, 14.7791));
            _vertices.Add(new GeoCoordinate(44.7389, 15.9122));
            _vertices.Add(new GeoCoordinate(47.3204, 17.1426));
            _vertices.Add(new GeoCoordinate(49.8886, 18.4893));
            _vertices.Add(new GeoCoordinate(50.0941, 17.5772));
            _vertices.Add(new GeoCoordinate(50.2914, 16.661));
            _vertices.Add(new GeoCoordinate(50.4818, 15.7354));
            _vertices.Add(new GeoCoordinate(50.6634, 14.8048));
            _vertices.Add(new GeoCoordinate(50.8413, 13.867));
            _vertices.Add(new GeoCoordinate(51.0093, 12.9197));
            _vertices.Add(new GeoCoordinate(51.1724, 11.9682));
            _vertices.Add(new GeoCoordinate(51.3223, 11.0092));
            _vertices.Add(new GeoCoordinate(51.4675, 10.0417));
            _vertices.Add(new GeoCoordinate(51.5997, 9.06597));
            _vertices.Add(new GeoCoordinate(51.728, 8.09084));
            _vertices.Add(new GeoCoordinate(51.8485, 7.10818));
            _vertices.Add(new GeoCoordinate(51.9604, 6.12087));
            _vertices.Add(new GeoCoordinate(52.0641, 5.12783));
            _vertices.Add(new GeoCoordinate(52.1595, 4.12945));
            _vertices.Add(new GeoCoordinate(52.2461, 3.13221));
            _vertices.Add(new GeoCoordinate(2.3247, 2.12399));
            _vertices.Add(new GeoCoordinate(52.3945, 1.11685));
            _vertices.Add(new GeoCoordinate(52.4562, 0.104198));
            _vertices.Add(new GeoCoordinate(49.8367, -0.291658));
            _vertices.Add(new GeoCoordinate(47.2156, -0.693546));
            _vertices.Add(new GeoCoordinate(44.5929, -1.10173));
            _vertices.Add(new GeoCoordinate(41.9739, -1.51452));

            var lastPoint = _vertices[_vertices.Count - 1];
            var isInside = false;
            var x = location.Longitude;
            foreach (var point in _vertices)
            {
                var x1 = lastPoint.Longitude;
                var x2 = point.Longitude;
                var dx = x2 - x1;

                if (Math.Abs(dx) > 180.0)
                {
                    // we have, most likely, just jumped the dateline (could do further validation to this effect if needed).  normalise the numbers.
                    if (x > 0)
                    {
                        while (x1 < 0)
                            x1 += 360;
                        while (x2 < 0)
                            x2 += 360;
                    }
                    else
                    {
                        while (x1 > 0)
                            x1 -= 360;
                        while (x2 > 0)
                            x2 -= 360;
                    }
                    dx = x2 - x1;
                }

                if ((x1 <= x && x2 > x) || (x1 >= x && x2 < x))
                {
                    var grad = (point.Latitude - lastPoint.Latitude) / dx;
                    var intersectAtLat = lastPoint.Latitude + ((x - x1) * grad);

                    if (intersectAtLat > location.Latitude)
                        isInside = !isInside;
                }
                lastPoint = point;
            }
            return isInside;
        }
    }
}

