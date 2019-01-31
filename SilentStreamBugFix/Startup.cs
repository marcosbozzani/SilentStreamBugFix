using Microsoft.Win32;
using SilentStreamBugFix.Properties;
using System.Windows.Forms;

namespace SilentStreamBugFix
{
    /// <summary>
    /// <remarks>
    /// References:
    /// https://stackoverflow.com/questions/25276418/how-to-run-my-winform-application-when-computer-starts/25276441#25276441
    /// </remarks>
    /// </summary>
    public class Startup
    {
        public static string Key => @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public Startup()
        {
            UpdateRegistry();
        }

        public bool Enabled
        {
            get
            {
                return Settings.Default.SystemStartup;
            }
            set
            {
                Settings.Default.SystemStartup = value;
                Settings.Default.Save();
                UpdateRegistry();
            }
        }

        private void UpdateRegistry()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(Key, true))
            {
                if (Enabled)
                {
                    key.SetValue(Application.ProductName, Setup.ExecutablePath);
                }
                else
                {
                    key.DeleteValue(Application.ProductName, false);
                }
            }
        }
    }
}
