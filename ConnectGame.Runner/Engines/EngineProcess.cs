using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;

namespace ConnectGame.Runner
{
    class EngineProcess : IEngineProcess
    {
        private readonly ILogger<EngineProcess> _logger;
        private Process _process;

        //private BufferBlock<string> _lines { get; set; }
        private Channel<string> _lines { get; set; }
        public IList<string> History { get; }

        public EngineProcess(ILogger<EngineProcess> logger)
        {
            _logger = logger;
            _lines = Channel.CreateUnbounded<string>();
            History = new List<string>();
        }

        public async Task StartAsync(string path)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = path;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            _process = Process.Start(startInfo);
            _process.OutputDataReceived += ProcessOnOutputDataReceived;
            _process.BeginOutputReadLine();
            //_process.BeginOutputReadLine();
            AppDomain.CurrentDomain.ProcessExit += (a, b) =>
            {
                Dispose();
            };
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            if (args.Data == null)
            {
                _process.OutputDataReceived -= ProcessOnOutputDataReceived;
                return;
            }

            //_logger.LogDebug("<<< {InMessage}", args.Data);
            _lines.Writer.TryWrite(args.Data);
        }

        public async Task<string> ReadLineAsync()
        {
            //var line = await _process.StandardOutput.ReadLineAsync();
            var line = await _lines.Reader.ReadAsync();

            _logger.LogDebug("<<< {InMessage}", line);
            History.Add($"<<< {line}");
            
            return line;
        }

        public async Task WriteLineAsync(string line)
        {
            _logger.LogDebug(">>> {OutMessage}", line);
            History.Add($">>> {line}");
            await _process.StandardInput.WriteLineAsync(line);
        }

        public void ClearHistory()
        {
            History.Clear();
        }

        public void Dispose()
        {
            _process?.Dispose();
        }


        public override string ToString()
        {
            return _process.StartInfo.FileName;
        }
    }
}