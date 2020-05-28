using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace or_satellite.Service
{
    public class TimedRemovalService: IHostedService
    {
        // Logger
        private readonly ILogger<TimedRemovalService> _logger;
        // Says how many days of data we store. Default is 7 days
        private int StoreTime = 7;

        public TimedRemovalService(ILogger<TimedRemovalService> logger, int? storeTime)
        {
            _logger = logger;
            if (storeTime.HasValue)
            {
                StoreTime = storeTime.Value;
            }
        }

        /// <summary>
        /// Start the async worker.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            var timer = new Timer(DoWork, null, TimeSpan.Zero, 
                TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Does the actual work
        /// </summary>
        /// <param name="state"></param>
        private void DoWork(object state)
        {
            // Check processed
            CheckAndDeleteDirectoriesOlderThen("/App/Copernicus/Processed");
            // Check Downloads
            CheckAndDeleteDirectoriesOlderThen("/App/Copernicus/Downloads");
            // Check Extraction
            CheckAndDeleteDirectoriesOlderThen("/App/Copernicus/Extraction");
        }
        
        private void CheckAndDeleteDirectoriesOlderThen(string direct)
        {
            //Retrieve all of directory files
            string[] directories = Directory.GetDirectories(direct);
            
            // Check each file directory that it is older then
            foreach (var directory in directories)
            {
                // Turn it into a directory information
                DirectoryInfo di = new DirectoryInfo(directory);
                // Check if the last time it was accessed was longer than the time we set for it.
                if (di.LastAccessTime < DateTime.Now.AddDays(-StoreTime))
                    // Delete the directory.
                    di.Delete(); 
            }
        }

        /// <summary>
        /// Stop the async worker
        /// </summary>
        /// <param name="stoppingToken">cancel token</param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");
            return Task.CompletedTask;
        }
    }
}