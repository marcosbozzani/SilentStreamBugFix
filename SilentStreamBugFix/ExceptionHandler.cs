using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SilentStreamBugFix
{
    public class ExceptionHandler
    {
        public static void Enable()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => Handle(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => Handle((Exception)e.ExceptionObject);
        }

        private static void Handle(Exception exception)
        {
            var logPath = Path.Combine(Application.StartupPath, Application.ProductName + ".log");
            File.AppendAllText(logPath, $"[{DateTime.Now.ToString("o")}] {exception}\r\n");
            var message = $"Error: {exception.Message}\n\nOpen log file?";
            var result = MessageBox.Show(message, Application.ProductName, MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Process.Start(logPath);
            }
            Environment.Exit(1);
        }
    }
}
