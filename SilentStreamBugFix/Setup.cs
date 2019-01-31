using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using static System.Environment.SpecialFolder;

namespace SilentStreamBugFix
{
    public class Setup
    {
        private static string ProgramsPath = GetSpecialFolderPath(Programs);
        private static string AppDataPath = GetSpecialFolderPath(ApplicationData);
        private static string LocalAppDataPath = GetSpecialFolderPath(LocalApplicationData);

        public static string IconPath => BasePath(Application.ProductName + ".ico");
        public static string ExecutablePath => BasePath(Application.ProductName + ".exe");
        public static string UninstallerPath => BasePath("Uninstall.cmd");
        public static string ExecutableStartMenuPath => StartMenuPath(Application.ProductName + ".url");
        public static string UninstallerStartMenuPath => StartMenuPath("Uninstall.url");

        public static void Install()
        {
            if (Application.ExecutablePath != ExecutablePath)
            {
                Directory.CreateDirectory(ProgramsPath);
                Directory.CreateDirectory(AppDataPath);
                Directory.CreateDirectory(LocalAppDataPath);

                File.Copy(Application.ExecutablePath, ExecutablePath, true);
                ExtractResources();
                GenerateUninstaller();
                StartMenuLinks();
            }
        }

        private static void ExtractResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName.EndsWith(".resources"))
                {
                    continue;
                }

                using (var input = assembly.GetManifestResourceStream(resourceName))
                using (var output = File.OpenWrite(ResourcePath(resourceName)))
                {
                    input.CopyTo(output);
                }
            }
        }

        private static void GenerateUninstaller()
        {
            using (var writer = new StreamWriter(UninstallerPath))
            {
                writer.WriteLine($"@echo Uninstalling {Application.ProductName}");
                writer.WriteLine($"taskkill /im \"{Application.ProductName}.exe\" /f");
                writer.WriteLine($"rd /s /q \"{ProgramsPath}\"");
                writer.WriteLine($"rd /s /q \"{AppDataPath}\"");
                writer.WriteLine($"rd /s /q \"{LocalAppDataPath}\"");
                writer.WriteLine($"reg delete \"HKEY_CURRENT_USER\\{Startup.Key}\" /v \"{Application.ProductName}\" /f");
                writer.WriteLine($"@pause");
            }
        }

        private static void StartMenuLinks()
        {
            using (var writer = new StreamWriter(ExecutableStartMenuPath))
            {
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=file:///" + ExecutablePath);
                writer.WriteLine("IconIndex=0");
                writer.WriteLine("IconFile=file:///" + IconPath);
            }

            using (var writer = new StreamWriter(UninstallerStartMenuPath))
            {
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=file:///" + UninstallerPath);
            }
        }

        public static string BasePath(string name)
        {
            return Path.Combine(AppDataPath, name);
        }

        public static string StartMenuPath(string menuItem)
        {
            return Path.Combine(ProgramsPath, menuItem);
        }

        public static string ResourcePath(string resourceName)
        {
            resourceName = resourceName.Substring(Application.ProductName.Length + 1);
            if (resourceName == "App.config")
            {
                resourceName = Application.ProductName + ".exe.config";
            }
            else if (resourceName == "icon.ico")
            {
                resourceName = Application.ProductName + ".ico";
            }
            return BasePath(resourceName);
        }

        private static string GetSpecialFolderPath(Environment.SpecialFolder folder)
        {
            return Path.Combine(Environment.GetFolderPath(folder), Application.ProductName);
        }
    }
}
