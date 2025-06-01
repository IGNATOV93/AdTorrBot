using AdTorrBot.BotTelegram.Db.Model.TorrserverModel;
using AdTorrBot.BotTelegram.Db;
using AdTorrBotTorrserverBot.BotTelegram;
using System.Text.Json;

namespace AdTorrBotTorrserverBot.Torrserver.BitTor
{
    public class BitTorrConfigation
    {
        static string nameProcesTorrserver = "TorrServer-linux-amd64";
        static string filePathTorrMain = TelegramBot.settingsJson.FilePathTorrserver;
        static string filePathTorrserverDb = @$"{filePathTorrMain}accs.db";
        static string filePathTorr = @$"{filePathTorrMain}{nameProcesTorrserver}";
        static string filePathSettingsJson = @$"{filePathTorrMain}settings.json";
        public static async Task ResetConfig()
        {
            await WriteConfig(new BitTorrConfig() { IdChat = TelegramBot.AdminChat });
            return;
        }
        public static async Task WriteConfig(BitTorrConfig config)
        {
            try
            {
                var wrapper = new BitTorrConfigWrapper(config);
                wrapper.BitTorr.Id = 0;
                var jsonString = JsonSerializer.Serialize(wrapper, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(filePathSettingsJson, jsonString);
                await SqlMethods.SetSettingsTorrProfile(config);
                Console.WriteLine($"Файл будет записан в: {filePathSettingsJson}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи JSON: {ex.Message}");
                return;
            }
            return;
        }
        public static async Task<BitTorrConfig> ReadConfig()
        {
            try
            {
                var jsonString = File.ReadAllText(filePathSettingsJson);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var config = JsonSerializer.Deserialize<BitTorrConfigWrapper>(jsonString, options)?.BitTorr;
                if (config == null)
                {
                    throw new Exception("Ошибка не удалось загрузить конфигурацию из JSON");
                }

                await SqlMethods.SetSettingsTorrProfile(config);

                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public class BitTorrConfigWrapper
        {
            public BitTorrConfig BitTorr { get; set; }
            public BitTorrConfigWrapper() { }
            public BitTorrConfigWrapper(BitTorrConfig config)
            {
                BitTorr = config;
            }
        }
    }
}
