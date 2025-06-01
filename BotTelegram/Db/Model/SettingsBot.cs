using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdTorrBot.BotTelegram.Db.Model
{
    public class SettingsBot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string IdChat { get; set; } = string.Empty;
        public double TimeZoneOffset { get; set; } = 3.0;
        public  string? LastChangeUid { get; set; }
        public void ChangeTimeZone(string direction)
        {
            if (direction == "+")
            {
                TimeZoneOffset = Math.Min(TimeZoneOffset + 1, 14.0);
            }
            else if (direction == "-")
            {
                TimeZoneOffset = Math.Max(TimeZoneOffset - 1, -12.0);
            }
        }
    }
}
