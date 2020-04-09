using System;
using Microsoft.AspNetCore.Mvc;
using or_satellite.Service;

namespace or_satellite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RunShellCommandController : ControllerBase
    {
        public RunShellCommand runShellCommand = new RunShellCommand();

        [HttpGet]
        public IActionResult getDate(string input)
        {
            var output = runShellCommand.DownloadFile();
            return new JsonResult(output);
        }
    }
}