using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;
namespace AdTorrBot.BotTelegram.Db.Model.TorrserverModel
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class ConfigOptionAttribute : Attribute
    {
        public string Key { get; }

        public ConfigOptionAttribute(string key)
        {
            Key = key;
        }
    }
    public class ServerArgsConfig
    {
      
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

       
        public string IdChat { get; set; } = string.Empty;

       
        [Description("Имя профиля")]
        public string NameProfileBot { get; set; } = "default";


        [ConfigOption("port")]
        [Description("Веб-порт сервера")]
        public int? Port { get; set; } = 8090; // --port

        [ConfigOption("logpath")]
        [Description("Путь для логов сервера")]
        public string? LogPath { get; set; } = "/opt/torrserver/torrserver.log"; // --logpath, -l

        [ConfigOption("path")]
        [Description("Путь к базе данных и конфигурации")]
        public string? Path { get; set; } = "/opt/torrserver"; // --path, -d

        [ConfigOption("httpauth")]
        [Description("Включение HTTP-аутентификации")]
        public bool HttpAuth { get; set; } = false; // --httpauth

        [ConfigOption("readonlymode")]
        [Description("Режим только для чтения")]
        public bool ReadOnlyMode { get; set; } = false; // --rdb, -r

        [ConfigOption("ssl")]
        [Description("Включение HTTPS")]
        public bool Ssl { get; set; } = false; // --ssl

        [ConfigOption("sslport")]
        [Description("HTTPS порт")]
        public int? SslPort { get; set; } = null; // --sslport

        [ConfigOption("sslcert")]
        [Description("Путь к SSL-сертификату")]
        public string? SslCert { get; set; } // --sslcert

        [ConfigOption("sslkey")]
        [Description("Путь к SSL-ключу")]
        public string? SslKey { get; set; } // --sslkey




        [ConfigOption("weblogpath")]
        [Description("Путь для логов веб-доступа")]
        public string? WebLogPath { get; set; } // --weblogpath

        [ConfigOption("dontkill")]
        [Description("Запрет завершения сервера")]
        public bool DontKill { get; set; } = false; // --dontkill

        [ConfigOption("ui")]
        [Description("Открыть интерфейс в браузере")]
        public bool Ui { get; set; } = false; // --ui

        [ConfigOption("torrentsdir")]
        [Description("Директория автозагрузки торрентов")]
        public string? TorrentsDir { get; set; } // --torrentsdir

        [ConfigOption("torrentaddr")]
        [Description("Адрес торрент-клиента")]
        public string? TorrentAddr { get; set; } // --torrentaddr

        [ConfigOption("pubipv4")]
        [Description("Публичный IPv4")]
        public string? PubIPv4 { get; set; } // --pubipv4, -4

        [ConfigOption("pubipv6")]
        [Description("Публичный IPv6")]
        public string? PubIPv6 { get; set; } // --pubipv6, -6

        [ConfigOption("searchwa")]
        [Description("Разрешить поиск без аутентификации")]
        public bool SearchWa { get; set; } = false; // --searchwa, -s

        [ConfigOption("help")]
        [Description("Показать справку")]
        public bool Help { get; set; } = false; // --help, -h

        [ConfigOption("version")]
        [Description("Показать версию программы")]
        public bool Version { get; set; } = false; // --version




        public override string ToString()
        {
            var properties = typeof(ServerArgsConfig).GetProperties().Skip(2);
            var sb = new StringBuilder();
            var i = 0;
            foreach (var property in properties)
            {
                i++;
                var description = GetDescription(property.Name);
                var value = property.GetValue(this)?.ToString();
                sb.AppendLine($"{i}) {description} ➡ {value}\r\n");
            }
            return sb.ToString();
        }

        public string GetDescription(string propertyName)
        {
            var property = typeof(ServerArgsConfig).GetProperty(propertyName);
            if (property == null) return null;
            var descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(property, typeof(DescriptionAttribute));
            return descriptionAttribute?.Description;
        }
    }

}
