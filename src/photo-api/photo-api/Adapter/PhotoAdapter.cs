using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace photo_api.Adapter
{
    public class PhotoAdapter
    {
        private static string Anaconda_Activate_Script = Startup.Configuration["AppSettings:AnacondaScript"];
        private static string AppDir = Startup.Configuration["AppSettings:AppDir"];
        private static string InputFolderRoot = Startup.Configuration["AppSettings:InputFolder"];
        private static string OutputFolderRoot = Startup.Configuration["AppSettings:OutputFolder"];
        private static string GpuParam = Startup.Configuration["AppSettings:GpuParam"];
        private static int MaxConcurrentProcesses = int.Parse(Startup.Configuration["AppSettings:MaxConcurrentProcesses"]);

        private readonly Semaphore _semaphore = new Semaphore(0, MaxConcurrentProcesses);

        private static void ProcessOutputLine(string type, string line, PhotoProcessResult status)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }
            Startup.EphemeralLog($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {type}: {line}");
            if (type == "stdout")
            {
                if (line.Contains("CUDA out of memory", StringComparison.InvariantCultureIgnoreCase)
                    || line.Contains("not enough memory", StringComparison.InvariantCultureIgnoreCase))
                {
                    status.ErrorCount++;
                    status.Errors.Add(line);
                }
            }
            else if (type == "stderr")
            {
                if (line.Contains("warning", StringComparison.InvariantCultureIgnoreCase)
                    || line.Contains("nn.Upsample", StringComparison.InvariantCultureIgnoreCase))
                {
                    // not an error, process wrongly logging success status to stderr
                }
                else
                {
                    status.ErrorCount++;
                    status.Errors.Add(line);
                }
            }
        }

        public PhotoProcessResult Execute(string traceId)
        {
            var signaled = _semaphore.WaitOne(300000);
            if (!signaled)
            {
                throw new Exception("Server still busy after wait period");
            }
            try
            {
                return ExecuteImpl(traceId);
            }
            finally 
            {
                _semaphore.Release();
            }
        }

        private PhotoProcessResult ExecuteImpl(string traceId)
        {
            var output = new StringBuilder();
            var status = new PhotoProcessResult();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"cmd.exe",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            process.ErrorDataReceived += new DataReceivedEventHandler(delegate (object sender, DataReceivedEventArgs e)
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    ProcessOutputLine("stderr", e.Data, status);
                }
            });
            process.OutputDataReceived += new DataReceivedEventHandler(delegate (object sender, DataReceivedEventArgs e)
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    ProcessOutputLine("stdout", e.Data, status);
                }
            });
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            var inputFolder = Path.Combine(InputFolderRoot, traceId);
            var outputFolder = Path.Combine(OutputFolderRoot, traceId);
            Directory.CreateDirectory(outputFolder);
            var command = @$"python run.py --input_folder ""{inputFolder}"" --output_folder ""{outputFolder}"" --GPU {GpuParam}";
            // TODO: param for sctrath detection          
            Startup.EphemeralLog($"Will execute: {command}", false);

            using (var sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine(Anaconda_Activate_Script);

                    sw.WriteLine(@$"CD ""{AppDir}""");
                    sw.WriteLine(command);
                    sw.WriteLine("conda deactivate");
                }
            }
            WaitOrKill(process, 30, command);
            status.OutputFolder = Path.Combine(outputFolder, "final_output");
            status.ExitCode = process.ExitCode;
            return status;
        }


        private void WaitOrKill(Process process, int minutes, string command)
        {
            if (!process.WaitForExit(milliseconds: minutes * 60 * 1000))
            {
                Startup.EphemeralLog($"---------------> PROCESS EXITED AFTER TIMEOUT. Killing process. Command: {command}", true);
                try
                {
                    process.Kill(true);
                }
                catch
                {
                    try { process.Kill(); } finally { }
                }
            }
            Startup.EphemeralLog($"--- Process Exited ---", true);
            process.CancelOutputRead();
            process.CancelErrorRead();
        }

    }

}
