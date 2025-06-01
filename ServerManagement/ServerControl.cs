using System.Diagnostics;

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


        public static void RebootServer()
        {
            try
            {
                Process.Start("reboot"); return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
