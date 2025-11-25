using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdTorrBot.ServerManagement
{
    internal class SystemMetrics
    {
        // CPU usage
        public static async Task<double> GetCpuUsageAsync()
        {
            try
            {
                var lines = await File.ReadAllLinesAsync("/proc/stat");
                var cpuLine = lines.FirstOrDefault(l => l.StartsWith("cpu "));
                if (cpuLine == null) return 0;

                var parts = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                   .Skip(1).Select(long.Parse).ToArray();

                long idle = parts[3] + parts[4]; // idle + iowait
                long total = parts.Sum();

                await Task.Delay(1000);

                lines = await File.ReadAllLinesAsync("/proc/stat");
                cpuLine = lines.FirstOrDefault(l => l.StartsWith("cpu "));
                parts = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                               .Skip(1).Select(long.Parse).ToArray();

                long idle2 = parts[3] + parts[4];
                long total2 = parts.Sum();

                double idleDelta = idle2 - idle;
                double totalDelta = total2 - total;

                if (totalDelta == 0) return 0;

                return Math.Round(100.0 * (1.0 - idleDelta / totalDelta), 2);
            }
            catch
            {
                return -1;
            }
        }


        // RAM usage
        public static async Task<(string used, string total)> GetRamUsageAsync()
        {
            try
            {
                var lines = await File.ReadAllLinesAsync("/proc/meminfo");
                long totalKb = ParseMeminfo(lines, "MemTotal");
                long availKb = ParseMeminfo(lines, "MemAvailable");
                long usedKb = totalKb - availKb;

                return ($"{usedKb / 1024} MB", $"{totalKb / 1024} MB");
            }
            catch
            {
                return ("ошибка", "ошибка");
            }
        }

        private static long ParseMeminfo(string[] lines, string key)
        {
            var line = lines.FirstOrDefault(l => l.StartsWith(key));
            if (line == null) return 0;
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return long.TryParse(parts[1], out var value) ? value : 0;
        }


        // Disk usage
        public static async Task<(string free, string total)> GetDiskUsageAsync()
        {
            try
            {
                var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name == "/");
                if (drive == null) return ("не найден", "не найден");

                long free = drive.AvailableFreeSpace;
                long total = drive.TotalSize;

                return (FormatGb(free), FormatGb(total));
            }
            catch
            {
                return ("ошибка", "ошибка");
            }
        }

        // Network traffic
        public static async Task<(string netIn, string netOut)> GetNetworkTrafficAsync()
        {
            try
            {
                var lines = await File.ReadAllLinesAsync("/proc/net/dev");
                var ethLine = lines.FirstOrDefault(l => l.Contains("eth0"))
                           ?? lines.FirstOrDefault(l => l.Contains("ens"));

                if (ethLine == null) return ("нет данных", "нет данных");

                var parts = ethLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                long rxBytes = long.Parse(parts[1]);
                long txBytes = long.Parse(parts[9]);

                return (FormatMb(rxBytes), FormatMb(txBytes));
            }
            catch
            {
                return ("ошибка", "ошибка");
            }
        }

        // Helpers
        private static string FormatMb(long kb) => $"{kb / 1024 / 1024} MB";
        private static string FormatGb(long bytes) => $"{bytes / 1024 / 1024 / 1024} GB";
    }
}

