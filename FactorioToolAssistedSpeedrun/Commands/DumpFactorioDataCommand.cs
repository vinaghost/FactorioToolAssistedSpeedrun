using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FactorioToolAssistedSpeedrun.Commands
{
    public partial class DumpFactorioDataCommand : ICommand, ICommandResult<string>
    {
        public required string FileName { get; init; }
        public string Result { get; private set; } = "Not loaded";

        public void Execute()
        {
            using var dumpDataProcess = new Process()
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
                    Result = match.Value;
                }

                dumpDataProcess.OutputDataReceived -= OutputDataReceivedHandler;
            }
            dumpDataProcess.OutputDataReceived += OutputDataReceivedHandler;

            using var dumpLocaleProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = FileName,
                    Arguments = "--dump-prototype-locale",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                }
            };

            dumpDataProcess.Start();
            dumpLocaleProcess.Start();
            dumpDataProcess.BeginOutputReadLine();
            dumpDataProcess.WaitForExit();
            dumpLocaleProcess.WaitForExit();
        }

        [GeneratedRegex(@"\d+\.\d+\.\d+")]
        private static partial Regex VersionMatcher();
    }
}