using AdTorrBot.BotTelegram.Db;
using AdTorrBot.BotTelegram.Db.Model.TorrserverModel;
using AdTorrBotTorrserverBot.BotTelegram;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AdTorrBotTorrserverBot.Torrserver.ServerArgs
{
    public class ServerArgsConfiguration
    {
        static string filePathTorrMain = TelegramBot.settingsJson.FilePathTorrserver;
        static string filePathTorrserverConfig = @$"{filePathTorrMain}torrserver.config"; 
        public static async Task ResetConfig()
        {
            await WriteConfigArgs(new ServerArgsConfig() { IdChat = TelegramBot.AdminChat,HttpAuth=true});
            return;
        }
        public static string SerializeConfigArgs(ServerArgsConfig config)
        {
            var parameters = new List<string>();
            foreach (var property in typeof(ServerArgsConfig).GetProperties())
            {
                var attribute = property.GetCustomAttribute<ConfigOptionAttribute>();
                if (attribute == null) continue; 

                var value = property.GetValue(config);
                if (value == null) continue; 
                if (property.PropertyType == typeof(bool))
                {
                    if ((bool)value) 
                    {
                        parameters.Add($"--{attribute.Key}");
                    }
                }
                else
                {
                    parameters.Add($"--{attribute.Key} {value}");
                }
            }
            return $"DAEMON_OPTIONS=\"{string.Join(" ", parameters)}\"";
        }
        public static async Task<ServerArgsConfig>  ReadConfigArgs()
        {
            try
            {
                if (!File.Exists(filePathTorrserverConfig))
                {
                    Console.WriteLine("Конфигурация (torrserver.config) не найдена. Создаём конфигурацию по умолчанию.");
                    var defaultConfig = new ServerArgsConfig();
                    await WriteConfigArgs(defaultConfig);
                   
                    return defaultConfig;
                }
                var configLine = File.ReadAllText(filePathTorrserverConfig);
                if (string.IsNullOrWhiteSpace(configLine))
                {
                    Console.WriteLine("Конфигурация (torrserver.config) пуста. Используем конфигурацию по умолчанию.");
                    var defaultConfig = new ServerArgsConfig();
                   await WriteConfigArgs(defaultConfig);
                    
                    return defaultConfig;
                }
                 var conf = ParseConfigArgs(configLine);
                  await SqlMethods.SetSettingsServerArgsProfile(conf);
                return conf;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении конфигурации(torrserver.config): {ex.Message}");
                throw;
            }
        }
        public static async Task WriteConfigArgs(ServerArgsConfig config)
        {
            try
            {
                var configLine = SerializeConfigArgs(config);
                if (File.Exists(filePathTorrserverConfig))
                {
                    var backupPath = $"{filePathTorrserverConfig}.bak";
                    File.Copy(filePathTorrserverConfig, backupPath, overwrite: true);
                    Console.WriteLine($"Резервная копия создана (torrserver.config.bak): {backupPath}");
                }
                File.WriteAllText(filePathTorrserverConfig, configLine);
                Console.WriteLine("Конфигурация(torrserver.config) успешно записана.");
                await SqlMethods.SetSettingsServerArgsProfile(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи конфигурации(torrserver.config): {ex.Message}");
                throw; 
            }
        }



        public static ServerArgsConfig ParseConfigArgs(string configLine)
        {
            var config = new ServerArgsConfig();
            var match = Regex.Match(configLine, @"DAEMON_OPTIONS\s*=\s*""([^""]*)""");
            if (!match.Success)
            {
                Console.WriteLine("Конфигурация(torrserver.config) не найдена или строка некорректна.");
                return config;
            }

            var args = match.Groups[1].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg.StartsWith("--"))
                {
                    var key = arg.Substring(2).ToLower(); 
                    string? value = null;
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    {
                        value = args[i + 1];
                        i++; 
                    }
                    else
                    {
                        value = "true";
                    }

                    var property = typeof(ServerArgsConfig).GetProperties()
                        .FirstOrDefault(p => p.GetCustomAttribute<ConfigOptionAttribute>()?.Key == key);

                    if (property != null)
                    {
                        try
                        {
                            var convertedValue = ConvertValue(value, property.PropertyType);
                            property.SetValue(config, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка парсинга ключа {key}: {ex.Message}");
                        }
                    }
                }
            }

            return config;
        }
        public static (string Protocol, int Port) GetProtocolAndPort(ServerArgsConfig config)
        {
            if (config.Ssl)
            {
                int port = config.SslPort ?? 443;
                return ("https", port);
            }
            else
            {
                int port = config.Port ?? 8090;
                return ("http", port);
            }
        }



        /// <summary>
        /// Преобразует строковое значение в указанный тип.
        /// </summary>
        private static object ConvertValue(string value, Type targetType)
        {
            return targetType switch
            {
                Type t when t == typeof(int?) => int.TryParse(value, out int intValue) ? intValue : null,
                Type t when t == typeof(long?) => long.TryParse(value, out long longValue) ? longValue : null,
                Type t when t == typeof(bool) => value.ToLower() == "true",
                Type t when t == typeof(string) => value,
                _ => throw new InvalidOperationException($"Неподдерживаемый тип: {targetType}")
            };
        }







    }

}
