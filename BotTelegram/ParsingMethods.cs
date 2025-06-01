using AdTorrBot.BotTelegram.Db.Model.TorrserverModel;

namespace AdTorrBot.BotTelegram
{
    
     public abstract class ParsingMethods
      {
        public static string EscapeForMarkdownV2(string text)
        {
            var charactersToEscape = new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
            foreach (var character in charactersToEscape)
            {
                text = text.Replace(character, $"\\{character}");
            }

            return text;
        }

        public static  string FormatProfilesList(List<Profiles> profiles, int countActive,int countAll,int countSkip, string sort)
        {
            int countInActive = Math.Max(0, countAll - (countActive));

            var result = $"📊 Профили: {countAll} (🟢{countActive}/🔴{countInActive})" +
                $"\r\n\r\n";
            var countActual = countSkip;
            for (int i = 0; i < profiles.Count; i++)
            {
                countActual++;
                var profile = profiles[i];
                var note = !string.IsNullOrEmpty(profiles[i].AdminComment) ? $"Заметка:  📌 {profiles[i].AdminComment}\r\n" : "";
                var uni = profile.UniqueId.ToString().Replace("-", "_");
                result += $"\n{countActual}) Логин: 👤 {profiles[i].Login}\r\n{note}" +
                    $"   {(profile.IsEnabled ? "🟢" : "🔴")} (до {profile.AccessEndDate?.ToString("yyyy-MM-dd") ?? "(не ограничено)"})\r\n"+
                $"/showlogpass_{profile.Login}_{profile.Password}\r\n";
                result += $"/edit_profile_{uni}\r\n" +
                    $"\r\n"
                    ; //
            }

            result += $"\nСортировка:\n{(sort == "sort_active" ? "🟢" : sort == "sort_inactive" ? "🔴" : "📅")} {sort}\n";
            return EscapeForMarkdownV2(result);
        }

        public static (int count, string sort) ParseOtherProfilesCallback(string callbackData)
        {
            if (!callbackData.Contains("OtherProfiles"))
            {
                throw new ArgumentException("Данные не соответствуют ожидаемому формату.");
            }
            var parts = callbackData.Split("OtherProfiles");
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out int count))
                {
                    string sort = parts[1];
                    return (count, sort);
                }
            }
            throw new ArgumentException("Невозможно распарсить данные.");
        }

        public static string GetExitMessage(string field)
        {
            switch (field)
            {
                case "FlagNoteOtherProfile":
                    return "Вы вышли из режима ввода заметки .";
                case "FlagLogin":
                    return "Вы вышли из режима ввода логина Torrserver. ✅";

                case "FlagPassword":
                    return "Вы вышли из режима ввода пароля Torrserver. ✅";

                case "FlagTorrSettCacheSize":
                    return "Вы вышли из режима ввода размера кеша (МБ). ✅";

                case "FlagTorrSettReaderReadAHead":
                    return "Вы вышли из режима ввода значения опережающего кеша (%). ✅";

                case "FlagTorrSettPreloadCache":
                    return "Вы вышли из режима ввода значения буфера предзагрузки (%). ✅";

                case "FlagTorrSettTorrentDisconnectTimeout":
                    return "Вы вышли из режима ввода тайм-аута отключения торрентов (сек). ✅";

                case "FlagTorrSettConnectionsLimit":
                    return "Вы вышли из режима ввода лимита соединений для торрентов. ✅";

                case "FlagTorrSettDownloadRateLimit":
                    return "Вы вышли из режима ввода ограничения скорости загрузки (кб/с). ✅";

                case "FlagTorrSettUploadRateLimit":
                    return "Вы вышли из режима ввода ограничения скорости отдачи (кб/с). ✅";

                case "FlagTorrSettPeersListenPort":
                    return "Вы вышли из режима ввода порта для входящих подключений. ✅";

                case "FlagTorrSettFriendlyName":
                    return "Вы вышли из режима ввода имени сервера DLNA. ✅";

                case "FlagTorrSettRetrackersMode":
                    return "Вы вышли из режима ввода режима ретрекеров. ✅";

                case "FlagTorrSettSslPort":
                    return "Вы вышли из режима ввода SSL порта. ✅";

                case "FlagTorrSettSslCert":
                    return "Вы вышли из режима ввода пути к SSL сертификату. ✅";

                case "FlagTorrSettSslKey":
                    return "Вы вышли из режима ввода пути к SSL ключу. ✅";

                case "FlagTorrSettTorrentsSavePath":
                    return "Вы вышли из режима ввода пути для сохранения торрентов. ✅";



                case "FlagServerArgsSettLogPath":
                    return "Вы вышли из режима ввода пути для логов сервера. ✅";

                case "FlagServerArgsSettPath":
                    return "Вы вышли из режима ввода пути к базе данных и конфигурации. ✅";

                case "FlagServerArgsSettSslPort":
                    return "Вы вышли из режима ввода HTTPS порта. ✅";

                case "FlagServerArgsSettSslCert":
                    return "Вы вышли из режима ввода пути к SSL-сертификату. ✅";

                case "FlagServerArgsSettSslKey":
                    return "Вы вышли из режима ввода пути к SSL-ключу. ✅";

                case "FlagServerArgsSettWebLogPath":
                    return "Вы вышли из режима ввода пути для логов веб-доступа. ✅";

                case "FlagServerArgsSettTorrentsDir":
                    return "Вы вышли из режима ввода директории автозагрузки торрентов. ✅";

                case "FlagServerArgsSettTorrentAddr":
                    return "Вы вышли из режима ввода адреса торрент-клиента. ✅";

                case "FlagServerArgsSettPubIPv4":
                    return "Вы вышли из режима ввода публичного IPv4. ✅";

                case "FlagServerArgsSettPubIPv6":
                    return "Вы вышли из режима ввода публичного IPv6. ✅";

                case "FlagLoginPasswordOtherProfile":
                    return "Вы вышли из режима ввода логина/пароля Torrserver. ✅";
                default:
                    return "Неизвестное поле. ✅";
            }
        }


        public static string UpdateTimeString(string time,int minutesToAdd)
        {
            DateTime dateTime = DateTime.ParseExact(time, "HH:mm", null);
            dateTime = dateTime.AddMinutes(minutesToAdd);
            return dateTime.ToString("HH:mm");
        }
        public static int ExtractTimeChangeValue(string valueInput)
        {
            var valueString = valueInput.Split("setAutoPassMinutes")[0].Trim(); // Получаем строку и убираем лишние пробелы
            int value;

            if (int.TryParse(valueString, out value))
            {
                Console.WriteLine($"Числовое значение: {value}");
            }
            else
            {
                value = 0;
                Console.WriteLine("Ошибка: значение не может быть преобразовано в число.");
            }
            return value;
        }
      }
}
