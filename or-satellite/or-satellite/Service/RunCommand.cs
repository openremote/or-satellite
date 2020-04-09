using System;
using System.Diagnostics;
using System.IO;
using Renci.SshNet;

namespace or_satellite.Service
{
    public class RunShellCommand
    {
        public string[] DownloadFile()
        {
            // var escapedArgs = cmd.Replace("\"", "\\\"");
            var downloadFile =
                "wget --no-check-certificate \"https://tinyurl.com/rlw3uhs\" -O filtered_merged_file.nc";

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
            string[] result = ProcessFile();
            return result;
        }

        private string[] ProcessFile()
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
            process.WaitForExit();
            string[] result = ReadFile();
            return result;
        }

        private string[] ReadFile()
        {
            var readFile =
                "cat Atmospheric_temperature_profile_formatted.txt";

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
            string result = process.StandardOutput.ReadToEnd().Replace("n", "\n");
            process.WaitForExit();
            string[] resultArray = SetResultToArray(result);
            DeleteFiles();
            return resultArray;
        }
        private void DeleteFiles()
        {
            var deleteFile1 =
                "rm Atmospheric_temperature_profile_formatted.txt";
            var deleteFile2 = "rm filtered_merged_file.nc";

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{deleteFile1}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            process.WaitForExit();
            process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{deleteFile2}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }

            };
            process.Start();
            process.WaitForExit();
        }


        private string[] SetResultToArray(string input)
        {
            string[] result = input.Split("\n");
            return result;
        }
    }
}