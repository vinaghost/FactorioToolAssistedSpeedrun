using FactorioToolAssistedSpeedrun.Commands.UI;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FactorioToolAssistedSpeedrun.Commands
{
    public static partial class DumpFactorioDataCommand
    {
        public static async Task<string> Execute(string fileName)
        {
            using var dumpDataProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = "--dump-data",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                }
            };
            var version = "Not loaded";
            var outputBuilder = new StringBuilder();
            void OutputDataReceivedHandler(object sender, DataReceivedEventArgs args)
            {
                if (string.IsNullOrEmpty(args.Data)) return;

                var match = VersionMatcher().Match(args.Data);
                if (match.Success)
                {
                    version = match.Value;
                    dumpDataProcess.OutputDataReceived -= OutputDataReceivedHandler;
                }
            }
            dumpDataProcess.OutputDataReceived += OutputDataReceivedHandler;

            using var dumpLocaleProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = "--dump-prototype-locale",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                }
            };

            dumpDataProcess.Start();
            dumpDataProcess.BeginOutputReadLine();
            dumpLocaleProcess.Start();

            await Task.WhenAll(Task.Run(dumpDataProcess.WaitForExit), Task.Run(dumpLocaleProcess.WaitForExit));
            return version;
        }

        [GeneratedRegex(@"\d+\.\d+\.\d+")]
        private static partial Regex VersionMatcher();
    }
}