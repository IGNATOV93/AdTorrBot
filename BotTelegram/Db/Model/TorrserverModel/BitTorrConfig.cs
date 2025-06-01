using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
namespace AdTorrBot.BotTelegram.Db.Model.TorrserverModel
{


    public class BitTorrConfig
    {
        [JsonIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [JsonIgnore]
        public string IdChat { get; set; } = string.Empty;
        [JsonIgnore]
        [Description("Имя профиля")]
        public string NameProfileBot { get; set; } = "default";

        [JsonConverter(typeof(CacheSizeConverter))]
        [Description("Размер кеша MB")]
        public long CacheSize
        {
            get => _cacheSize / (1024 * 1024); 
            set => _cacheSize = value * 1024 * 1024; 
        }
        private long _cacheSize = 67108864; 


        [Description("Опережающий кеш %")]
        public int ReaderReadAHead { get; set; } = 95;

        [Description("Буфер предзагрузки %")]
        public int PreloadCache { get; set; } = 50;

        [Description("Вкл. поиск по RuTor")]
        public bool EnableRutorSearch { get; set; } = false;

        [Description("Включение отладки")]
        public bool EnableDebug { get; set; } = false;

        [Description("Использование диска")]
        public bool UseDisk { get; set; } = false;

        [Description("Включение IPv6")]
        public bool EnableIPv6 { get; set; } = false;

        [Description("Отключение TCP")]
        public bool DisableTCP { get; set; } = false;

        [Description("Отключение μTP (MTP)")]
        public bool DisableUTP { get; set; } = false;

        [Description("Отключение PEX")]
        public bool DisablePEX { get; set; } = false;

        [Description("Принуд. шифрование")]
        public bool ForceEncrypt { get; set; } = false;

        [Description("Тайм-аут откл. торрентов сек.")]
        public int TorrentDisconnectTimeout { get; set; } = 30;

        [Description("Торрент-соединения кол-во")]
        public int ConnectionsLimit { get; set; } = 25;

        [Description("Отключение DHT")]
        public bool DisableDHT { get; set; } = false;


        [JsonConverter(typeof(CacheSizeConverter))]
        [Description("Ограничение скорости загрузки мб/сек")]
        public long DownloadRateLimit
        {
            get => _downloadRateLimit /1024;
            set => _downloadRateLimit = value * 1024; 
        }
        private long _downloadRateLimit = 0;


        [JsonConverter(typeof(CacheSizeConverter))]
        [Description("Ограничение скорости отдачи мб/сек")]
        public long UploadRateLimit 
        {
            get => _uploadRateLimit / 1024;
            set => _uploadRateLimit = value * 1024;
        } 
        private long _uploadRateLimit = 0;


        [Description("Порт для входящих подключений")]
        public int PeersListenPort { get; set; } = 0;

        [Description("Отключение UPNP")]
        public bool DisableUPNP { get; set; } = false;

        [Description("Включение DLNA")]
        public bool EnableDLNA { get; set; } = false;

        [Description("Имя сервера DLNA")]
        public string FriendlyName { get; set; } = "";

        [Description("Включить быстрый режим чтения")]
        public bool ResponsiveMode { get; set; } = false;

        [Description("Режим ретрекеров")]
        public int RetrackersMode { get; set; } = 1;

        [Description("SSL порт")]
        public int SslPort { get; set; } = 0;

        [Description("SSL сертификат")]
        public string SslCert { get; set; } = "";

        [Description("SSL ключ")]
        public string SslKey { get; set; } = "";

        [Description("Отключение отдачи")]
        public bool DisableUpload { get; set; } = false;

        [Description("Удаление кеша при сбросе")]
        public bool RemoveCacheOnDrop { get; set; } = false;

        [Description("Путь сохр. торрентов")]
        public string TorrentsSavePath { get; set; } = "";

        public override string ToString()
        {
            var properties = typeof(BitTorrConfig).GetProperties().Skip(2);
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
            var property = typeof(BitTorrConfig).GetProperty(propertyName);
            if (property == null) return null;

            var descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(property, typeof(DescriptionAttribute));
            return descriptionAttribute?.Description;
        }
    }


}
