using or_satellite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace or_satellite.Service
{
    public class DatasetFootprintCheck
    {
        public bool Contains(GeoCoordinate location, List<GeoCoordinate> _vertices)
        {
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

