using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using AdTorrBot.ServerManagement;

namespace AdTorrBot.BotTelegram.Db.Model
{
    public class SettingsTorrserverBot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? idChat { get; set; }

        public string? Login { get; set; } = "adTorrBot";
        public string? Password { get; set; } 
        public bool IsActiveAutoChange { get; set; } = false;
        public string TimeAutoChangePassword { get; set; } = "19:00";

        public bool IsTorrserverAutoRestart { get; set; } = true;

        public string TorrserverRestartTime { get; set; } = "20:00";


        public string AutoBackupTime { get; set; } = "21:00"; 
        public bool IsAutoBackupEnabled { get; set; } = true;
        public override string ToString()
        {
            var localTime = ServerInfo.GetLocalServerTime();
            return

                $"Статус автосмены пароля: {(IsActiveAutoChange ? "✅" : "❌")}\r\n" +
                $"\u23F0 Автосмена пароля : {TimeAutoChangePassword} \r\n";
        }
    }
}
