using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace or_satellite.Models
{
    public class SearchResultModel
    {
        public bool succes = false;
        public string latitude;
        public string longitude;
        public string date;
        public SearchResultEnum searchResult; 
    }
}
