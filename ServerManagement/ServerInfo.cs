using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;

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
    }
}
