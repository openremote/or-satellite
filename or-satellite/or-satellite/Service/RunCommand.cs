using System;
using System.Diagnostics;
using System.IO;
using Renci.SshNet;

namespace or_satellite.Service
{
    public class RunShellCommand
    {
        public string DownloadFile()
        {
            // var escapedArgs = cmd.Replace("\"", "\\\"");
            var downloadFile =
                "curl https://filebin.net/ucm0428h6yv6vfz8/filtered_merged_file.nc?t=19wm9v0i -o filtered_merged_file.nc";

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{downloadFile}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            // string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            string result = ProcessFile();
            return result;
        }

        private string ProcessFile()
        {
            var processFile =
                "ncks -s \"%.3f\\n\" -F -H -C -v atmospheric_temperature_profile filtered_merged_file.nc  > Atmospheric_temperature_profile_formatted.txt";

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{processFile}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            // string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            string result = ReadFile();
            return result;
        }

        private string ReadFile()
        {
            var readFile =
                "tail -n 100 Atmospheric_temperature_profile_formatted.txt";

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{readFile}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}