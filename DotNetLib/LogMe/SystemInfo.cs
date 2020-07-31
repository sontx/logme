using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;

namespace LogMeLib
{
    internal static class SystemInfo
    {
        public static string GetInfo()
        {
            var computerInfo = new ComputerInfo();
            var builder = new StringBuilder();
            var divider = "----------------------------------------";
            builder.AppendLine("Device mode: " + (IsRunningInVM() ? "VM" : "Real Computer"));
            builder.AppendLine("Windows: " + computerInfo.OSFullName);
            builder.AppendLine("Release Id: " + GetReleaseId());
            builder.AppendLine(divider);
            builder.AppendLine("Resolution: " + GetResolution());
            builder.AppendLine("RAM: " + GetRAM(computerInfo));
            builder.AppendLine("Storage: " + GetStorage());
            builder.AppendLine("Language: " + CultureInfo.CurrentCulture.DisplayName);
            builder.AppendLine(divider);
            builder.AppendLine("App name: " + GetAppName());
            builder.AppendLine("App version: " + GetFileVersion());
            return builder.ToString();
        }

        private static bool IsRunningInVM()
        {
            using (var searcher = new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
            {
                using (var items = searcher.Get())
                {
                    foreach (var item in items)
                    {
                        string manufacturer = item["Manufacturer"].ToString().ToLower();
                        if ((manufacturer == "microsoft corporation" && item["Model"].ToString().IndexOf("VIRTUAL", StringComparison.InvariantCultureIgnoreCase) >= 0)
                            || manufacturer.Contains("vmware")
                            || item["Model"].ToString() == "VirtualBox")
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static string GetReleaseId()
        {
            return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "").ToString();
        }

        private static string GetResolution()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                using (var result = searcher.Get())
                {
                    var objects = result.Cast<ManagementObject>();
                    var managementObject = objects.FirstOrDefault();
                    if (managementObject == null)
                        return "Unknown";
                    var x = managementObject["CurrentHorizontalResolution"].ToString();
                    var y = managementObject["CurrentVerticalResolution"].ToString();
                    return $"{x}x{y}";
                }
            }
        }

        private static string GetRAM(ComputerInfo computerInfo)
        {
            return $"{ReadableFileSize(computerInfo.AvailablePhysicalMemory)} / {ReadableFileSize(computerInfo.TotalPhysicalMemory)}";
        }

        private static string GetStorage()
        {
            var currentDriveInfo = GetCurrentDriveInfo();
            return currentDriveInfo == null
                ? "Unknown"
                : $"{ReadableFileSize((ulong)currentDriveInfo.AvailableFreeSpace)} / {ReadableFileSize((ulong)currentDriveInfo.TotalSize)}";
        }

        private static DriveInfo GetCurrentDriveInfo()
        {
            var currentPath = Path.GetFullPath(Environment.CurrentDirectory);
            var currentDriveLetter = currentPath.Substring(0, 1).ToUpper();
            return DriveInfo.GetDrives().FirstOrDefault(drive => drive.Name.ToUpper().StartsWith(currentDriveLetter));
        }

        private static string ReadableFileSize(ulong size)
        {
            if (size <= 0) return "0";
            var units = new string[] { "B", "kB", "MB", "GB", "TB" };
            int digitGroups = (int)(Math.Log10(size) / Math.Log10(1024));
            var value = size / Math.Pow(1024, digitGroups);
            return $"{value:F1} {units[digitGroups]}";
        }

        public static string GetAppName()
        {
            var assembly = Assembly.GetEntryAssembly();
            var productAttr = assembly.GetCustomAttribute<AssemblyProductAttribute>();
            return productAttr != null ? productAttr.Product : Path.GetFileNameWithoutExtension(assembly.Location);
        }

        private static string GetFileVersion()
        {
            var assembly = Assembly.GetEntryAssembly();
            var fileVersionAttr = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            return fileVersionAttr != null ? fileVersionAttr.Version : "Unknown";
        }
    }
}