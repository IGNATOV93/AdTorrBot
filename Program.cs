using AdTorrBot.BotTelegram.BotSettings;
using AdTorrBotTorrserverBot.BotTelegram;
using AdTorrBotTorrserverBot.Torrserver;

public class MainClass
{
    public static TimeZoneInfo moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
    public static DateTime nowTimeMsk = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, moscowTimeZone);
    static async Task Main()
    {
        while (!await Torrserver.IsServiceInstalled("torrserver"))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Сервис TorrServer не найден на сервере. Проверьте установку.");
            Console.WriteLine("⚠️ Установите его и нажмите 1 для повторной проверки.");
            Console.WriteLine("Нажмите 2 для выхода из установвки этого бота.");
            Console.ResetColor();

            var input = Console.ReadLine(); 
            if (input != null && input == "2")
            {
                Environment.Exit(0);
            }
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ TorrServer найден! Продолжаем...");
        Console.ResetColor();
        if (!BotSettingsMethods.ValidateOrCreateSettings())
        {
           BotSettingsMethods.ConfigureBot(); 

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Настройки успешно завершены! Запускаем бота...");
            Console.ResetColor();

        }
 
        await TelegramBot.StartBot();
        await Task.Delay(-1);
    }
}