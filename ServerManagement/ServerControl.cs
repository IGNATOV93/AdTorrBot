using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace AdTorrBot.ServerManagement
{
    public abstract class ServerControl
    {

        public static async Task SetBbrState(bool enable)
        {
            try
            {
                string path = "/etc/sysctl.conf";
                string content = File.ReadAllText(path);
                string bbrDef = "net.core.default_qdisc=fq";
                string bbrIpv4 = "net.ipv4.tcp_congestion_control=bbr";

                if (enable)
                {
                    content = System.Text.RegularExpressions.Regex.Replace(content, @"net\.core\.default_qdisc\s*=\s*fq", "");
                    content = System.Text.RegularExpressions.Regex.Replace(content, @"net\.ipv4\.tcp_congestion_control\s*=\s*bbr", "");
                    File.WriteAllText(path, content);
                    await File.AppendAllTextAsync(path, "\n" + bbrDef);
                    await File.AppendAllTextAsync(path, "\n" + bbrIpv4);
                }
                else
                {
                    content = System.Text.RegularExpressions.Regex.Replace(content, @"net\.core\.default_qdisc\s*=\s*fq", "");
                    content = System.Text.RegularExpressions.Regex.Replace(content, @"net\.ipv4\.tcp_congestion_control\s*=\s*bbr", "");
                    File.WriteAllText(path, content);
                }
                Process processSysctl = new Process();
                processSysctl.StartInfo.FileName = "sysctl";
                processSysctl.StartInfo.Arguments = "-p";
                processSysctl.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public static void RestartBotService()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "systemctl",
                    Arguments = "restart adtorrbot.service",
                    UseShellExecute = false
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public static void RebootServer()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "sudo",
                    Arguments = "reboot",
                    UseShellExecute = false
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public static string GetPublicIp()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                var ipProps = ni.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var ip = addr.Address.ToString();
                        if (!ip.StartsWith("10.") &&
                            !ip.StartsWith("192.168.") &&
                            !ip.StartsWith("172.16.") &&
                            !ip.StartsWith("127."))
                        {
                            return ip;
                        }
                    }
                }
            }
            return "Не определен!";
        }

    }
}
