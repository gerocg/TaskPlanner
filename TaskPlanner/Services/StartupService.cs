using Microsoft.Win32;
using System;
using System.IO;

namespace TaskPlanner.Services
{
    public static class StartupService
    {
        private const string AppName = "TaskPlanner";

        private const string RegistryKeyPath =
            @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static void Enable()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                RegistryKeyPath, writable: true);

            if (key == null)
                throw new InvalidOperationException(
                    "No se pudo acceder a la configuración de inicio.");

            string executablePath =
                Environment.ProcessPath
                ?? throw new InvalidOperationException(
                    "No se pudo determinar la ruta de la aplicación.");

            key.SetValue(
                AppName,
                $"\"{Path.GetFullPath(executablePath)}\"");
        }

        public static void Disable()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                RegistryKeyPath, writable: true);

            key?.DeleteValue(AppName, throwOnMissingValue: false);
        }

        public static bool IsEnabled()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                RegistryKeyPath, writable: false);

            return key?.GetValue(AppName) != null;
        }
    }
}
