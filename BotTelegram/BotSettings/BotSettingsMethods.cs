using Newtonsoft.Json;
using AdTorrBot.BotTelegram.BotSettings.Model;
using AdTorrBotTorrserverBot.BotTelegram;

namespace AdTorrBot.BotTelegram.BotSettings
{
    public  abstract class BotSettingsMethods
    {
        private static string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public  enum SettingsField
        {
            YourBotTelegramToken,
            AdminChatId,
            TimeAutoChangePassword,
            FilePathTorrserverBd,
            FilePathTor,
            IsActiveAutoChange,
            LoginDefaultTorrserver
        }
        public static void ConfigureBot()
        {
            while (true)
            {
                ShowErrorMessage("❌ Настройки бота не заполнены.");

                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1️ Заполнить настройки через консоль");
                Console.WriteLine("2️ Изменил сам settings.json, перезапустить бота");
                Console.WriteLine("3️ Выйти из режима запуска");

                Console.Write("Введите номер действия: ");
                string userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1":
                        ConfigureBotSettings();
                        return;

                    case "2":
                        Console.WriteLine("⚠️ Отредактируйте settings.json вручную и перезапустите установку бота.");
                        Environment.Exit(0);
                        return;

                    case "3":
                        Console.WriteLine("🚪 Выход из режима запуска...");
                        Environment.Exit(0);
                        return;

                    default:
                        ShowErrorMessage("❌ Неверный выбор. Попробуйте снова.");
                        break;
                }
            }
        }

        public static bool ValidateOrCreateSettings()
        {
            if (!File.Exists(path))
            {
                ShowWarningMessage("⚠️ Файл настроек не найден. Создаю новый settings.json...");
                SaveSettings(new BotSettingsJson { YourBotTelegramToken = "", AdminChatId = "", FilePathTorrserver = "/opt/torrserver/" });
                ShowWarningMessage("⚠️ settings.json создан! Заполните его вручную.");
                return false;
            }

            var settings = LoadSettings();
            if (!settings.Validate(out List<string> missingFields))
            {
                ShowErrorMessage("❌ Настройки бота не заполнены.");
                Console.WriteLine($"⚠️ Заполните параметры: {string.Join(", \r\n", missingFields)}");
                Console.WriteLine("🔄 Отредактируйте settings.json вручную и попробуйте снова.");
                return false;
            }

            ShowSuccessMessage("✅ Настройки бота корректны.");
            return true;
        }

        private static void ConfigureBotSettings()
        {
            while (true)
            {
                Console.Write("Введите Telegram токен: ");
                string token = Console.ReadLine();

                Console.Write("Введите AdminChatId: ");
                string adminChatId = Console.ReadLine();

                ShowInfoMessage("\n🔍 Проверяем введенные данные...");
                Console.WriteLine($"➡ Telegram Token: {token}");
                Console.WriteLine($"➡ Admin Chat ID: {adminChatId}");
                Console.WriteLine($"➡ File Path Torrserver: /opt/torrserver/ (Можно поменять в settings.json)");

                Console.Write("\n✅ Данные верны? (Yes/No/Back): ");
                string confirmation = Console.ReadLine()?.ToLower();

                if (confirmation == "yes")
                {
                    BotSettingsMethods.SaveSettings(new BotSettingsJson { YourBotTelegramToken = token, AdminChatId = adminChatId, FilePathTorrserver = "/opt/torrserver/" });
                    ShowSuccessMessage("✅ Настройки сохранены! Перезапустите бота.");
                    return;
                }
                else if (confirmation == "back")
                {
                    ShowWarningMessage("🔄 Возвращаемся назад в меню...");
                    break;
                }
                else
                {
                    ShowWarningMessage("🔄 Попробуем снова.");
                }
            }
        }
        private static void ShowErrorMessage(string message) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(message); Console.ResetColor(); }
        private static void ShowWarningMessage(string message) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine(message); Console.ResetColor(); }
        private static void ShowSuccessMessage(string message) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(message); Console.ResetColor(); }
        private static void ShowInfoMessage(string message) { Console.ForegroundColor = ConsoleColor.Blue; Console.WriteLine(message); Console.ResetColor(); }


        public static void UpdateSettings(SettingsField field, string newValue)
        {
            var settings = LoadSettings();

            if (settings == null) return;

            switch (field)
            {
                case SettingsField.YourBotTelegramToken:
                    settings.YourBotTelegramToken = newValue;
                    break;
                case SettingsField.AdminChatId:
                    settings.AdminChatId = newValue;
                    break;
                case SettingsField.FilePathTorrserverBd:
                    settings.FilePathTorrserver = newValue;
                    break;
               
            }

            SaveSettings(settings);
            TelegramBot.settingsJson = LoadSettings();
            Console.WriteLine($"{field} успешно обновлено.");
        }
        public static BotSettingsJson LoadSettings()
        {
            var json = File.ReadAllText(path);
            var settings = JsonConvert.DeserializeObject<BotSettingsJson>(json);
            if (settings == null)
            {
                throw new InvalidOperationException("Не удалось загрузить настройки: настройки равны null.");
            }
            return settings;
        }
        public static void SaveSettings(BotSettingsJson settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
