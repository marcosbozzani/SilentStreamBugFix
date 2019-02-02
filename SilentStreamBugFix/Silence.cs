using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace SilentStreamBugFix
{
    /// <summary>
    /// <remarks>
    /// References:
    /// https://github.com/mvaneerde/blog/tree/master/silence
    /// https://blogs.msdn.microsoft.com/matthew_van_eerde/2008/12/10/sample-playing-silence-via-wasapi-event-driven-pull-mode/
    /// </remarks>
    /// </summary>
    public class Silence
    {
        private Process process;

        public Silence()
        {
            var bits = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            var name = Path.Combine(Application.StartupPath, ($"silence-{bits}.exe"));

            process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = name,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                }
            };
        }

        public void Start()
        {
            process.Start();
            ChildProcessTracker.AddProcess(process);
        }

        public void Stop()
        {
            process.Kill();
        }

        public void Reload()
        {
            if (!process.HasExited)
            {
                Stop();
            }

            Start();
        }
    }
}
