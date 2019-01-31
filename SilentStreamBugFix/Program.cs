using SilentStreamBugFix.Properties;
using System;
using System.Threading;
using System.Windows.Forms;

namespace SilentStreamBugFix
{
    public class Program : ApplicationContext
    {
        static Mutex mutex = new Mutex(true, "{04AD4EDC-3418-486E-AC09-AD91C6297141}");

        [STAThread]
        public static void Main(string[] args)
        {
            // single instance application
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                ExceptionHandler.Enable();
                Setup.Install();
                Application.Run(new Program());
            }
        }

        private Silence silence;
        private NotifyIcon trayIcon;
        private Startup systemStartup;

        public Program()
        {
            silence = new Silence();
            systemStartup = new Startup();
            trayIcon = new NotifyIcon()
            {
                Visible = true,
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[]
                {
                    new MenuItem("Reload", Reload),
                    new MenuItem("Run on system startup", Startup)
                    {
                        Checked = systemStartup.Enabled
                    },
                    new MenuItem("Exit", Exit)
                })
            };

            silence.Start();
        }

        private void Reload(object sender, EventArgs e)
        {
            silence.Reload();
        }

        private void Startup(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            menuItem.Checked = !menuItem.Checked;
            systemStartup.Enabled = menuItem.Checked;
        }

        private void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
