using System;
using System.Windows.Forms;

namespace SilentStreamBugFix.Setup
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ExceptionHandler.Enable();
            Installer.Install();
            MessageBox.Show(ProductInfo.Name + " installed", ProductInfo.FullName,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
