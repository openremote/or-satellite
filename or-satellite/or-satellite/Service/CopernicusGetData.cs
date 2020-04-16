using System;
using CopernicusOpenCSharp;
using System.Threading.Tasks;
namespace or_satellite.Service
{
    public class CopernicusGetData
    {
        public async Task<CopernicusService> GetDataAsync(string username, string password)
        {
            CopernicusService service = new CopernicusService(username, password);

            string id = "'fea3cd38-918d-4974-8586-2578cbb07844'";
            var test = await service.GetDataAsync(id: id);

            return service;
        }
 

    }
}