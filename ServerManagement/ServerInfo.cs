using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using AdTorrBotTorrserverBot.Torrserver.ServerArgs;
using AdTorrBotTorrserverBot.Torrserver;
using AdTorrBot.BotTelegram.Db;
using System.Diagnostics;

namespace AdTorrBot.ServerManagement
{
    public class ServerInfo
    {
        public static bool IsPortAvailable(int port)
        {
            try
            {
                using (var tcpListener = new TcpListener(IPAddress.Any, port))
                {
                    tcpListener.Start();
                    Console.WriteLine($"Порт {port} свободен.");
                    return true;
                }
            }
            catch (SocketException)
            {
                Console.WriteLine($"Порт {port} занят.");
                return false; 
            }
        }
        public static bool CheckBBRConfig()
        {
            string path = "/etc/sysctl.conf";
            if (!File.Exists(path))
            {
                return false;
            }
            string content = File.ReadAllText(path);
            bool hasCongestionControl = Regex.IsMatch(
                content,
                @"net\.ipv4\.tcp_congestion_control\s*=\s*bbr"
            );

            return hasCongestionControl;
        }



        public static double GetLocalServerTimeTimeZone()
        {
            DateTime localTime = DateTime.Now;
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            TimeSpan offset = localZone.GetUtcOffset(localTime);
            return offset.Hours + offset.Minutes / 60.0;
        }
        public static string GetLocalServerTime()
        {
            return DateTime.Now.ToString("HH:mm"); 
        }
        public static async Task<bool> GetAutoBackupSettings()
        {
            var settings = await SqlMethods.GetSettingsTorrserverBot();
            return settings.IsAutoBackupEnabled;
        }

        public static async Task<bool> GetTorrserverAutoRunSettingAsync()
        {
            var settings = await SqlMethods.GetSettingsTorrserverBot();
            return settings.IsTorrserverAutoRestart;
        }

        public static async Task<bool> IsAutoChangePasswordEnabled()
        {
            var settings = await SqlMethods.GetSettingsTorrserverBot();
            return settings.IsActiveAutoChange;
        }
        public static TimeSpan GetSystemUptime()
        {
            string uptimeText = File.ReadAllText("/proc/uptime").Split(' ')[0];
            double uptimeSeconds = double.Parse(uptimeText, System.Globalization.CultureInfo.InvariantCulture);
            return TimeSpan.FromSeconds(uptimeSeconds);
        }
        public static TimeSpan? GetProcessUptime(string processName)
        {
            var process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (process == null) return null;
            return DateTime.Now - process.StartTime;
        }
        public static TimeSpan GetBotUptime()
        {
            DateTime start = Process.GetCurrentProcess().StartTime;
            return DateTime.Now - start;
        }
        public static string GetBotVersion()
        {
            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "version.txt");
                if (File.Exists(path))
                {
                    return File.ReadAllText(path).Trim();
                }
                else
                {
                    return "неизвестна";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения версии: {ex.Message}");
                return "ошибка";
            }
        }
        public static string GetOsVersion()
        {
            try
            {
                string path = "/etc/os-release";
                if (File.Exists(path))
                {
                    var lines = File.ReadAllLines(path);
                    var nameLine = lines.FirstOrDefault(l => l.StartsWith("PRETTY_NAME="));
                    if (nameLine != null)
                    {
                        return nameLine.Replace("PRETTY_NAME=", "").Trim('"');
                    }
                }
                return "неизвестна";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения версии ОС: {ex.Message}");
                return "ошибка";
            }
        }
        public static string GetTorrserverVersion()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/opt/torrserver/TorrServer-linux-amd64",
                        Arguments = "-v",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

               
                var line = output.Split('\n').FirstOrDefault(l => l.Contains("TorrServer"));
                return string.IsNullOrWhiteSpace(line) ? "неизвестна" : line.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения версии TorrServer: {ex.Message}");
                return "ошибка";
            }
        }

        public static async Task<string> GetStatusAsync()
        {
            try
            {
                var logMessage = new System.Text.StringBuilder();

                // 🌐 Torrserver URL
                var configArgs = await ServerArgsConfiguration.ReadConfigArgs();
                var (protocol, port) = ServerArgsConfiguration.GetProtocolAndPort(configArgs);
                string ip = ServerControl.GetPublicIp();
                string url = $"{protocol}://{ip}:{port}";
                logMessage.AppendLine($"🌐 Torrserver доступен по адресу:\r\n{url}\r\n");

                // 👤 Авторизация главного пользователя
                string logPassMain = Torrserver.TakeMainAccountTorrserver() ?? "";
                string login = "";
                string password = "";

                if (!string.IsNullOrWhiteSpace(logPassMain))
                {
                    var parts = logPassMain.Split(new[] { ':' }, 2);
                    login = parts[0].Trim();
                    password = parts.Length > 1 ? parts[1].Trim() : "";
                }
                logMessage.AppendLine("Основной логин:пароль");
                logMessage.AppendLine($"{logPassMain}\r\n");

                // 🚀 Проверка BBR
                bool bbrEnabled = ServerInfo.CheckBBRConfig();
                logMessage.AppendLine($"🚀 BBR: {(bbrEnabled ? "включён" : "не настроен")}");

                // 🕒 Локальное время
                string localTime = ServerInfo.GetLocalServerTime();
                double tzOffset = ServerInfo.GetLocalServerTimeTimeZone();
                logMessage.AppendLine($"🕒 Локальное время сервера: {localTime} (UTC{(tzOffset >= 0 ? "+" : "")}{tzOffset})\r\n");

                // ⚙️ Статусы задач
                var isTorrserverAutoRunEnabled = await GetTorrserverAutoRunSettingAsync();
                var isAutoBackupRunEnabled = await GetAutoBackupSettings();
                bool isAutoChangePasswordEnabled = await IsAutoChangePasswordEnabled();

                logMessage.AppendLine($"⚙️ Авто-перезапуск Torrserver: {(isTorrserverAutoRunEnabled ? "🟢 Вкл" : "🔴 Выкл")}");
                logMessage.AppendLine($"💾 Авто-бекап: {(isAutoBackupRunEnabled ? "🟢 Вкл" : "🔴 Выкл")}");
                logMessage.AppendLine($"🔑 Авто-смена пароля: {(isAutoChangePasswordEnabled ? "🟢 Вкл" : "🔴 Выкл")}\r\n");



                // 👥 Пользователи
                int countAllProfiles = await SqlMethods.GetCountAllProfiles();
                int countActive = await SqlMethods.GetActiveProfilesCount();
                int countInactive = Math.Max(0, countAllProfiles - countActive);

                logMessage.AppendLine("👥 Пользователи:");
                logMessage.AppendLine($"• Активные: {countActive}");
                logMessage.AppendLine($"• Неактивные: {countInactive}\r\n");

                // ⏱ Аптайм
                var botUptime = GetBotUptime();
                var torrUptime = GetProcessUptime("TorrServer-linux-amd64")
                 ?? GetProcessUptime("TorrServer-linux-arm64");
                var vpsUptime = GetSystemUptime();

                logMessage.AppendLine("⏱ Аптайм:");
                logMessage.AppendLine($"• Бот работает: {botUptime.Days} д {botUptime.Hours} ч {botUptime.Minutes} мин");
                if (torrUptime != null)
                    logMessage.AppendLine($"• Torrserver работает: {torrUptime.Value.Days} д {torrUptime.Value.Hours} ч {torrUptime.Value.Minutes} мин");
                else
                    logMessage.AppendLine("• Torrserver не найден");
                logMessage.AppendLine($"• VPS работает: {vpsUptime.Days} д {vpsUptime.Hours} ч {vpsUptime.Minutes} мин\r\n");

                // 🖥 Системные метрики
                var cpuUsage = await SystemMetrics.GetCpuUsageAsync();
                var (usedRam, totalRam) = await SystemMetrics.GetRamUsageAsync();
                var (freeDisk, totalDisk) = await SystemMetrics.GetDiskUsageAsync();
                var (netIn, netOut) = await SystemMetrics.GetNetworkTrafficAsync();

                logMessage.AppendLine("🖥 Системные метрики:");
                logMessage.AppendLine($"• CPU: {cpuUsage}%");
                logMessage.AppendLine($"• RAM: {usedRam} / {totalRam}");
                logMessage.AppendLine($"• Диск: {freeDisk} свободно из {totalDisk}");
                logMessage.AppendLine($"• Трафик: ↑ {netIn} / ↓ {netOut}\r\n");

                // 📦 Версии ПО
                logMessage.AppendLine("📦 Версии ПО:");
                string botVersion = GetBotVersion();
                logMessage.AppendLine($"• AdTorrBot {botVersion}");

                string torrVersion = GetTorrserverVersion();
                logMessage.AppendLine($"• Torrserver {torrVersion}");

                string osVersion = GetOsVersion();
                logMessage.AppendLine($"• {osVersion}");

                return logMessage.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сборе статуса: {ex.Message}\n{ex.StackTrace}");
                return "⚠️ Ошибка: не удалось считать информацию";
            }
        }
    }
}
