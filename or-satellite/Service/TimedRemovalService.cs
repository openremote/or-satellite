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
        private readonly ILogger<TimedRemovalService> _logger;

        public TimedRemovalService(ILogger<TimedRemovalService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            var timer = new Timer(DoWork, null, TimeSpan.Zero, 
                TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            if (Directory.Exists($"/App/Copernicus/Processed/{DateTime.Now.AddDays(-7)}"))
            {
                Directory.Delete($"/App/Copernicus/Processed/{DateTime.Now.AddDays(-7)}");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");
            return Task.CompletedTask;
        }
    }
}