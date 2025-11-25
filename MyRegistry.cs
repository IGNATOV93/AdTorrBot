using AdTorrBot.BotTelegram.Db;
using FluentScheduler;
using AdTorrBotTorrserverBot.BotTelegram;
using AdTorrBot.ServerManagement;
using AdTorrBot.BotTelegram.Db.Model;



namespace AdTorrBotTorrserverBot
{
    public class MyRegistry : Registry
    {
        public MyRegistry()
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var logMessage = new System.Text.StringBuilder();

            var isTorrserverAutoRunEnabled = await ServerInfo.GetTorrserverAutoRunSettingAsync();
            var isAutoBackupRunEnabled = await ServerInfo.GetAutoBackupSettings();
            bool isAutoChangePasswordEnabled = await ServerInfo.IsAutoChangePasswordEnabled();

            string torrserverStatus = isTorrserverAutoRunEnabled ? "🟢 Вкл" : "🔴 Выкл";
            string autoBackupStatus = isAutoBackupRunEnabled ? "🟢 Вкл" : "🔴 Выкл";
            string autoChangePasswordStatus = isAutoChangePasswordEnabled ? "🟢 Вкл" : "🔴 Выкл";

            logMessage.AppendLine($"⚙️ Авто-перезапуск Torrserver: {torrserverStatus}");
            logMessage.AppendLine($"💾 Авто-бекап: {autoBackupStatus}");
            logMessage.AppendLine($"🔑 Авто-смена пароля: {autoChangePasswordStatus}");

            if (isAutoChangePasswordEnabled)
            {
                var time = await GetScheduledTimeAsync("AutoChangePassword");
                logMessage.AppendLine($"🕒 Запланирована авто-смена пароля на {time.hours:D2}:{time.minutes:D2}");

                // ✅ Запуск задачи с учетом часового пояса
                var adjustedTime = await GetTimeWithOffsetAsync("AutoChangePassword");
                Schedule(async () => await Torrserver.Torrserver.AutoChangeAccountTorrserver())
                    .ToRunEvery(1)
                    .Days()
                    .At(adjustedTime.hours, adjustedTime.minutes);
            }

            if (isTorrserverAutoRunEnabled)
            {
                var time = await GetScheduledTimeAsync("Torrserver");
                logMessage.AppendLine($"🔥 Запланирован авто-перезапуск Torrserver на {time.hours:D2}:{time.minutes:D2}");

                // ✅ Запуск задачи с учетом часового пояса
                var adjustedTime = await GetTimeWithOffsetAsync("Torrserver");
                Schedule(async () => await RunTorrserverTask())
                    .ToRunEvery(1)
                    .Days()
                    .At(adjustedTime.hours, adjustedTime.minutes);
            }

            if (isAutoBackupRunEnabled)
            {
                var time = await GetScheduledTimeAsync("Backup");
                logMessage.AppendLine($"📅 Запланирован авто-бекап на {time.hours:D2}:{time.minutes:D2}");

                // ✅ Запуск задачи с учетом часового пояса
                var adjustedTime = await GetTimeWithOffsetAsync("Backup");
                Schedule(async () => await RunAutoBackupTask())
                    .ToRunEvery(1)
                    .Days()
                    .At(adjustedTime.hours, adjustedTime.minutes);
            }

            Console.WriteLine(logMessage.ToString());
            await BotTelegram.TelegramBot.SendMessageToAdmin("📋 Текущий статус задач на сегодня:\r\n" + logMessage.ToString());
        }
        private async Task<(int hours, int minutes)> GetTimeWithOffsetAsync(string taskType)
        {
            var setBot = await SqlMethods.GetSettingBot();
            double offset = setBot.TimeZoneOffset; // Часовой пояс из настроек
            double serverTimeZone = ServerInfo.GetLocalServerTimeTimeZone(); // Часовой пояс сервера

           // Console.WriteLine($"🕒 Часовой пояс из настроек: {setBot.TimeZoneOffset}");
           // Console.WriteLine($"🖥️ Часовой пояс сервера: {serverTimeZone}");

            var settings = await LoadSettingsAsync();
            string rawTime = taskType switch
            {
                "Torrserver" => settings.TorrserverRestartTime,
                "Backup" => settings.AutoBackupTime,
                "AutoChangePassword" => settings.TimeAutoChangePassword,
                _ => throw new ArgumentException($"Неизвестный тип задачи: {taskType}")
            };

            int baseHours = int.Parse(rawTime.Split(":")[0]);
            int baseMinutes = int.Parse(rawTime.Split(":")[1]);

            // Корректируем на разницу между настройками и сервером
            double timezoneDifference = serverTimeZone - setBot.TimeZoneOffset;
            int adjustedHours = baseHours + (int)Math.Floor(timezoneDifference);
            int adjustedMinutes = baseMinutes + (int)Math.Round((timezoneDifference - (int)timezoneDifference) * 60);

            // Корректируем выход за границы суток
            if (adjustedMinutes >= 60)
            {
                adjustedHours += adjustedMinutes / 60;
                adjustedMinutes %= 60;
            }
            else if (adjustedMinutes < 0)
            {
                adjustedHours -= 1;
                adjustedMinutes += 60;
            }

            adjustedHours = (adjustedHours + 24) % 24; // Если выходит за границы суток, корректируем

           // Console.WriteLine($"⏳ Исправленное время для запуска на сервере {taskType}: {adjustedHours:D2}:{adjustedMinutes:D2}");
            return (adjustedHours, adjustedMinutes);
        }






        private async Task<(int hours, int minutes)> GetScheduledTimeAsync(string taskType)
        {
            var settings = await LoadSettingsAsync();
            string rawTime = taskType switch
            {
                "Torrserver" => settings.TorrserverRestartTime,
                "Backup" => settings.AutoBackupTime,
                "AutoChangePassword" => settings.TimeAutoChangePassword,
                _ => throw new ArgumentException($"Неизвестный тип задачи: {taskType}")
            };

            int baseHours = int.Parse(rawTime.Split(":")[0]);
            int baseMinutes = int.Parse(rawTime.Split(":")[1]);

            // ✅ Логирование без учета смещения
            Console.WriteLine($"🕒 Исходное время для {taskType}: {baseHours:D2}:{baseMinutes:D2}");

            return (baseHours, baseMinutes);
        }

        private async Task RunAutoBackupTask()
        {
            await TelegramBot.StartAutoBackup();
        }

        private async Task RunTorrserverTask()
        {
            await Torrserver.Torrserver.RebootingTorrserver();
            await BotTelegram.TelegramBot.SendMessageToAdmin("✅ Torrserver успешно перезагружен!");
        }

        private static async Task<SettingsTorrserverBot> LoadSettingsAsync()
        {
            return await SqlMethods.GetSettingsTorrserverBot();
        }

        private int AdjustHours(int baseHours, double timeZoneOffset)
        {
            int finalHours = baseHours + (int)timeZoneOffset;
            return finalHours % 24;
        }

        private int AdjustMinutes(int baseMinutes, double timeZoneOffset)
        {
            int additionalMinutes = (int)((timeZoneOffset - (int)timeZoneOffset) * 60);
            int finalMinutes = baseMinutes + additionalMinutes;

            return finalMinutes % 60;
        }
    }
}



