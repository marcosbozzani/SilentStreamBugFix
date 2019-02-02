using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using static System.Environment.SpecialFolder;

namespace SilentStreamBugFix.Setup
{
    public class Installer
    {
        private static string ProgramsPath = GetSpecialFolderPath(Programs);
        private static string AppDataPath = GetSpecialFolderPath(ApplicationData);
        private static string LocalAppDataPath = GetSpecialFolderPath(LocalApplicationData);
        
        public static string IconPath => BasePath(ProductInfo.Name + ".ico");
        public static string ExecutablePath => BasePath(ProductInfo.Name + ".exe");
        public static string UninstallerPath => BasePath("Uninstall.cmd");
        public static string ExecutableStartMenuPath => StartMenuPath(ProductInfo.Name + ".lnk");
        public static string UninstallerStartMenuPath => StartMenuPath("Uninstall.lnk");
        public static string StartupRegKey => WindowsRegistry("Run");
        public static string UninstallRegKey => WindowsRegistry("Uninstall");

        public static void Install()
        {
            KillProcess(ProductInfo.Name);

            Directory.CreateDirectory(ProgramsPath);
            Directory.CreateDirectory(AppDataPath);
            Directory.CreateDirectory(LocalAppDataPath);
            
            ExtractResources();
            RegisterUninstaller();
            StartMenuLinks();
            
            Process.Start(ExecutablePath);
        }

        private static void KillProcess(string name)
        {
            var processes = Process.GetProcessesByName(name);
            foreach (var process in processes)
            {
                process.Kill();
            }
            Thread.Sleep(500);
        }

        public static bool IsInstalled()
        {
            return System.IO.File.Exists(ExecutablePath);
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
                using (var output = System.IO.File.OpenWrite(ResourcePath(resourceName)))
                {
                    input.CopyTo(output);
                }
            }
        }

        private static void RegisterUninstaller()
        {
            System.IO.File.WriteAllText(UninstallerPath, "@echo off\n" +
                $"taskkill /im \"{ProductInfo.Name}.exe\" /f\n" +
                $"rd /q /s \"{ProgramsPath}\"\n" +
                $"rd /q /s \"{LocalAppDataPath}\"\n" +
                $"rd /q /s \"{LocalAppDataPath}.Setup\"\n" +
                $"reg delete \"HKCU\\{StartupRegKey}\" /v \"{ProductInfo.Name}\" /f\n" +
                $"reg delete \"HKCU\\{UninstallRegKey}\\{ProductInfo.Guid}\" /f\n" +
                $"rd /q /s \"{AppDataPath}\"\n"
            );            
            using (var parent = Registry.CurrentUser.OpenSubKey(UninstallRegKey, true))
            {
                if (parent == null)
                {
                    throw new Exception("Uninstall registry key not found.");
                }

                try
                {
                    RegistryKey key = null;

                    try
                    {
                        key = parent.OpenSubKey(ProductInfo.Guid, true) ?? parent.CreateSubKey(ProductInfo.Guid);

                        if (key == null)
                        {
                            throw new Exception($"Unable to create uninstaller '{UninstallRegKey}\\{ProductInfo.Guid}'");
                        }
                        
                        key.SetValue("DisplayName", ProductInfo.Name);
                        key.SetValue("ApplicationVersion", ProductInfo.FullName);
                        key.SetValue("Publisher", "marcosbozzani");
                        key.SetValue("DisplayIcon", "\"" + ExecutablePath + "\"");
                        key.SetValue("DisplayVersion", ProductInfo.Version);
                        key.SetValue("URLInfoAbout", "https://github.com/marcosbozzani/SilentStreamBugFix");
                        key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                        key.SetValue("UninstallString", "\"" + UninstallerPath + "\" /uninstall");
                        key.SetValue("QuietUninstallString", "\"" + UninstallerPath + "\" /uninstall /quiet");
                        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                    }
                    finally
                    {
                        if (key != null)
                        {
                            key.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred writing uninstall information to the registry", ex);
                }
            }
        }

        private static void StartMenuLinks()
        {
            CreateShortcut(ExecutableStartMenuPath, ExecutablePath, IconPath);
            CreateShortcut(UninstallerStartMenuPath, UninstallerPath);
        }

        private static void CreateShortcut(string shortcutPath, string targetPath, string iconPath = null)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;

            if (iconPath != null)
            {
                shortcut.IconLocation = iconPath;
            }

            shortcut.Save();
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
            resourceName = resourceName.Substring((ProductInfo.Name + ".Setup").Length + 1);
            if (resourceName == "App.config")
            {
                resourceName = ProductInfo.Name + ".exe.config";
            }
            else if (resourceName == "AppIcon.ico")
            {
                resourceName = ProductInfo.Name + ".ico";
            }
            return BasePath(resourceName);
        }

        private static string GetSpecialFolderPath(Environment.SpecialFolder folder)
        {
            return Path.Combine(Environment.GetFolderPath(folder), ProductInfo.Name);
        }

        private static string WindowsRegistry(string name)
        {
            return @"SOFTWARE\Microsoft\Windows\CurrentVersion\" + name;
        }
    }
}
