using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace XMC_Flasher.FrameWorks
{
    internal static class CommonMethods
    {
        /// <summary>
        /// Run a process async
        /// </summary>
        /// <param name="process"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
        {
            if (process.HasExited)
            {
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource<object>();

            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);

            if (cancellationToken != default)
            {
                cancellationToken.Register(() => tcs.SetCanceled());
            }

            return process.HasExited ? Task.CompletedTask : tcs.Task;
        }

        public static Process StartNewProcess(this string program, string parameter = "")
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = $"\"{program}\"";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = parameter;
            p.StartInfo.RedirectStandardError = true;
            p.Start();
            return p;
        }
    }
}
