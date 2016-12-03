using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LclDckr
{
    internal static class ProcessExtensions
    {
        public static Task<Process> WaitForExitAsync(this Process process)
        {
            var tcs = new TaskCompletionSource<Process>();

            process.Exited += (sender, args) =>
            {
                tcs.TrySetResult(process);
            };

            return tcs.Task;
        }

        public static Process ThrowForError(this Process process)
        {
            if (process.ExitCode != 0)
            {
                throw new Exception($"command failed: {process.StandardError.ReadToEnd()}");
            }

            return process;
        }
    }
}