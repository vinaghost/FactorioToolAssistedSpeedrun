using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FactorioToolAssistedSpeedrun.Commands
{
    public partial class DumpFactorioDataCommand : ICommand
    {
        public required string FileName { get; init; }
        public string Version { get; private set; } = "Not loaded";

        public async Task Execute()
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = FileName,
                    Arguments = "--dump-data",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                }
            };

            var outputBuilder = new StringBuilder();
            void OutputDataReceivedHandler(object sender, DataReceivedEventArgs args)
            {
                if (string.IsNullOrEmpty(args.Data)) return;

                var match = VersionMatcher().Match(args.Data);
                if (match.Success)
                {
                    Version = match.Value;
                }

                process.OutputDataReceived -= OutputDataReceivedHandler;
            }
            process.OutputDataReceived += OutputDataReceivedHandler;

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        [GeneratedRegex(@"\d+\.\d+\.\d+")]
        private static partial Regex VersionMatcher();
    }
}