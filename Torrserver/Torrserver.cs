using System.Diagnostics;
using AdTorrBotTorrserverBot.BotTelegram;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using AdTorrBot.BotTelegram.Db;
using AdTorrBot.BotTelegram.Db.Model.TorrserverModel;



namespace AdTorrBotTorrserverBot.Torrserver
{
    public abstract class Torrserver

    {
        static string  nameProcesTorrserver = "TorrServer-linux-amd64";
        static string filePathTorrMain = TelegramBot.settingsJson.FilePathTorrserver;
        static string filePathTorrserverDb = @$"{filePathTorrMain}accs.db";
        static string filePathTorr = @$"{filePathTorrMain}{nameProcesTorrserver}";
        static string filePathSettingsJson = @$"{filePathTorrMain}settings.json";


        #region MainPforile
        public static async Task AutoChangeAccountTorrserver()
        {
            var settings = await SqlMethods.GetSettingsTorrserverBot();
            if (settings != null&&settings.IsActiveAutoChange==true)
            {
                var inlineKeyboarDeleteMessageOnluOnebutton = new InlineKeyboardMarkup(new[]
                  {new[]{InlineKeyboardButton.WithCallbackData("Скрыть \U0001F5D1", "deletemessages")}});

                await ChangeMainAccountTorrserver("","",false,true);
                await TelegramBot.client.SendTextMessageAsync(TelegramBot.AdminChat, $"Произведена автосмена пароля сервера \U00002705\r\n" +
                                                                                      $"\U0001F570   {DateTime.Now}", replyMarkup: inlineKeyboarDeleteMessageOnluOnebutton);
                await TelegramBot.client.SendTextMessageAsync(TelegramBot.AdminChat, $"{TakeMainAccountTorrserver()}", replyMarkup: inlineKeyboarDeleteMessageOnluOnebutton);
            }
           
            return;
        }


        //Смена пароля/логина главного аккаунта(профиля torrserver)
        public static async Task ChangeMainAccountTorrserver(string login, string password, bool setLogin, bool setPassword)
        {
            var newParolRandom = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var settingsTorr = await SqlMethods.GetSettingsTorrserverBot();
            string newPassword = string.IsNullOrEmpty(password) ? settingsTorr.Password : password;
            string newLogin = string.IsNullOrEmpty(login) ? settingsTorr.Login : login;

            if (setLogin && string.IsNullOrEmpty(login))
            {
                newLogin = new string(Enumerable.Repeat(chars, 10)
                                  .Select(s => s[newParolRandom.Next(s.Length)]).ToArray());
            }

            if (setPassword && string.IsNullOrEmpty(password))
            {
                newPassword = new string(Enumerable.Repeat(chars, 10)
                                  .Select(s => s[newParolRandom.Next(s.Length)]).ToArray());
            }

            await SqlMethods.SetLoginPasswordSettingsTorrserverBot(newLogin, newPassword);
            string newAccount = $"\"{newLogin}\":\"{newPassword}\"";
            Console.WriteLine(newAccount);

            try
            {
                if (File.Exists(filePathTorrserverDb))
                {
                    var lines = File.ReadAllLines(filePathTorrserverDb).ToList();
                    string content = string.Join("", lines).Trim(); // Считываем всё содержимое как одну строку

                    if (content.StartsWith("{") && content.EndsWith("}"))
                    {
                        content = content.Substring(1, content.Length - 2); // Убираем внешние фигурные скобки

                        // Разбиваем на аккаунты по запятой
                        var accounts = content.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        if (accounts.Count > 0)
                        {
                            // Заменяем первый аккаунт
                            accounts[0] = newAccount;
                        }
                        else
                        {
                            // Если список пуст, добавляем новый аккаунт
                            accounts.Add(newAccount);
                        }

                        // Собираем всё обратно в строку
                        string updatedContent = $"{{{string.Join(",", accounts)}}}";
                        File.WriteAllText(filePathTorrserverDb, updatedContent);
                    }
                    else
                    {
                        // Если файл пуст или формат некорректен, создаем новый файл с одним аккаунтом
                        File.WriteAllText(filePathTorrserverDb, $"{{{newAccount}}}");
                    }
                }
                else
                {
                    // Если файла нет, создаем новый файл с одним аккаунтом
                    File.WriteAllText(filePathTorrserverDb, $"{{{newAccount}}}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении файла: {ex.Message}");
            }

            await RebootingTorrserver();
        }
        public static async Task<bool> IsServiceInstalled(string serviceName)
        {
            var checkProcess = new ProcessStartInfo
            {
                FileName = "systemctl",
                Arguments = $"status {serviceName}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(checkProcess))
                {
                    if (process == null) return false;

                    using (var reader = process.StandardOutput)
                    {
                        string output = await reader.ReadToEndAsync();
                        return output.Contains("Loaded: loaded");
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        public static async Task ControlTorrserver(bool enable)
        {
            string serviceName = "torrserver";
            string action = enable ? "start" : "stop";

            // Проверяем, установлен ли сервис
            if (!await IsServiceInstalled(serviceName))
            {
                Console.WriteLine($"❌ Сервис {serviceName} не найден на сервере. Проверьте установку.");
                return;
            }

            Console.WriteLine($@"🔄 {(enable ? "Запуск" : "Остановка")} Torrserver...");

            var processInfo = new ProcessStartInfo
            {
                FileName = "systemctl",
                Arguments = $"{action} {serviceName}",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(processInfo))
                {
                    await process.WaitForExitAsync();
                    Console.WriteLine($"✅ Torrserver успешно {action} через systemd.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка при {action} Torrserver: {ex.Message}");
                return;
            }

            if (enable)
            {
                Console.WriteLine("🚀 Torrserver запустился! Обновляем профили...");
                await Task.Delay(1000);
                await UpdateAllProfilesFromConfig();
            }
        }


        public static async Task RebootingTorrserver()
        {
            string serviceName = "torrserver";

            // Проверяем, установлен ли сервис
            if (!await IsServiceInstalled(serviceName))
            {
                Console.WriteLine($"❌ Сервис {serviceName} не найден на сервере. Проверьте установку.");
                return;
            }

            Console.WriteLine($"✅ Сервис {serviceName} найден, выполняем перезагрузку...");

            var stopProcess = new ProcessStartInfo
            {
                FileName = "systemctl",
                Arguments = $"stop {serviceName}",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(stopProcess))
                {
                    await process.WaitForExitAsync();
                    Console.WriteLine("✅ TorrServer успешно остановлен через systemd.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка остановки TorrServer: {ex.Message}");
                return;
            }

            await Task.Delay(2000);

            var startProcess = new ProcessStartInfo
            {
                FileName = "systemctl",
                Arguments = $"start {serviceName}",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                Process.Start(startProcess);
                Console.WriteLine("🚀 TorrServer успешно запустился через systemd!");

                await Task.Delay(1000);
                await UpdateAllProfilesFromConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка запуска TorrServer: {ex.Message}");
            }
        }

        public static string? ParseMainLoginFromTorrserverProfile(string? profileString)
        {
            // Проверяем строку на пустоту и наличие разделителя
            if (!string.IsNullOrWhiteSpace(profileString) && profileString.Contains(":"))
            {
                // Разделяем строку по символу ':'
                string[] parts = profileString.Split(':');

                // Убедимся, что обе части строки содержат значения
                if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    return parts[0]; // Возвращаем левую часть строки как логин
                }
            }

            // Если строка не соответствует формату или пустая
            return null;
        }

        public static string TakeMainAccountTorrserver()
        {
            try
            {
                // Попытка открытия файла
                using (StreamReader reader = new StreamReader(filePathTorrserverDb))
                {
                    // Чтение и предварительная очистка содержимого файла
                    string content = reader.ReadToEnd().Trim().Replace("\r", "").Replace("\n", "").Replace(" ", "");

                    if (content.StartsWith("{") && content.EndsWith("}"))
                    {
                        // Удаляем внешние фигурные скобки
                        content = content.Substring(1, content.Length - 2);

                        // Разбиваем содержимое по запятой
                        var accounts = content.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        if (accounts.Length > 0)
                        {
                            // Берем первую строку
                            string firstAccount = accounts[0].Trim();

                            // Извлекаем логин и пароль
                            int keyStartIndex = firstAccount.IndexOf("\"") + 1;
                            int keyEndIndex = firstAccount.IndexOf("\":");
                            int valueStartIndex = firstAccount.IndexOf(":\"") + 2;
                            int valueEndIndex = firstAccount.LastIndexOf("\"");

                            if (keyStartIndex >= 0 && keyEndIndex > keyStartIndex &&
                                valueStartIndex > keyEndIndex && valueEndIndex > valueStartIndex)
                            {
                                string login = firstAccount.Substring(keyStartIndex, keyEndIndex - keyStartIndex);
                                string password = firstAccount.Substring(valueStartIndex, valueEndIndex - valueStartIndex);

                                return $"{login}:{password}";
                            }

                            return "Ошибка извлечения логина или пароля."; // Краткое сообщение об ошибке
                        }

                        return "Нет данных для обработки."; // Краткое сообщение об ошибке
                    }

                    return "Некорректный формат файла."; // Краткое сообщение об ошибке
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка чтения файла: {ex.Message}"; // Краткое сообщение об ошибке
            }
        }

        #endregion MainProfile

        #region OtherProfiles
        public static async Task<bool> DeleteProfileByLogin(string loginToDelete)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePathTorrserverDb))
                {
                    // Считываем весь файл как одну строку
                    string content = reader.ReadToEnd().Trim();

                    if (content.StartsWith("{") && content.EndsWith("}"))
                    {
                        // Убираем внешние фигурные скобки
                        content = content.Substring(1, content.Length - 2);

                        // Разбиваем строки по запятой
                        var accounts = content.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        var filteredAccounts = new List<string>();

                        foreach (var account in accounts)
                        {
                            int keyStartIndex = account.IndexOf("\"") + 1;
                            int keyEndIndex = account.IndexOf("\":");

                            if (keyStartIndex >= 0 && keyEndIndex > keyStartIndex)
                            {
                                string login = account.Substring(keyStartIndex, keyEndIndex - keyStartIndex);

                                // Исключаем аккаунт, если логин совпадает с loginToDelete
                                if (!login.Equals(loginToDelete, StringComparison.OrdinalIgnoreCase))
                                {
                                    filteredAccounts.Add(account); // Добавляем только те записи, которые не совпадают
                                }
                            }
                        }

                        // Формируем новую строку и записываем обратно в файл
                        string updatedContent = "{" + string.Join(",", filteredAccounts) + "}";
                        await File.WriteAllTextAsync(filePathTorrserverDb, updatedContent);

                        return true; // Удаление успешно
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении профиля: {ex.Message}");
            }

            return false; // В случае ошибки
        }



        //Обловляем что у нас есть в конфиге пользователи с бд данными .
        public static async Task<bool> UpdateAllProfilesFromConfig()
        {
            try
            {
                // Шаг 1: Получить все профили из базы данных
                var allProfiles = await SqlMethods.GetAllProfilesNoSkip();
                var activeProfiles = allProfiles.Where(profile => profile.IsEnabled).ToList();
                var inactiveProfiles = allProfiles.Where(profile => !profile.IsEnabled).ToList();

                // Шаг 2: Считать данные из конфигурационного файла
                var profilesFromConfig = ReadProfilesFromConfig();

                // Шаг 3: Удалить неактивные профили из конфигурационного файла
                profilesFromConfig = profilesFromConfig
                    .Where(configProfile => !inactiveProfiles.Any(inactive => inactive.Login == configProfile.Login))
                    .ToList();

                // Шаг 4: Добавить только активные профили в конфигурацию (если их там нет)
                var uniqueLoginsConfig = new HashSet<string>(profilesFromConfig.Select(profile => profile.Login));
                var profilesToAddToConfig = activeProfiles
                    .Where(activeProfile => !uniqueLoginsConfig.Contains(activeProfile.Login))
                    .ToList();

                if (profilesToAddToConfig.Any())
                {
                    profilesFromConfig.AddRange(profilesToAddToConfig);
                }

                // Шаг 5: Записать обновлённые данные в конфигурационный файл
                await WriteAllProfilesToConfig(profilesFromConfig);

                // Шаг 6: Обновить базу данными из конфигурации (при необходимости)
                var uniqueLoginsDb = new HashSet<string>(allProfiles.Select(profile => profile.Login));
                var profilesToAddToDb = profilesFromConfig
                    .Where(configProfile => !uniqueLoginsDb.Contains(configProfile.Login))
                    .ToList();

                if (profilesToAddToDb.Any())
                {
                    await SqlMethods.UpdateOrAddProfilesAsync(profilesToAddToDb);
                }
                await SqlMethods.UpdateIsActiveProfiles();
                return true; // Синхронизация успешно завершена
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке профилей: {ex.Message}");
                return false;
            }
        }



        public static  List<Profiles> ReadProfilesFromConfig()
        {
            var profiles = new List<Profiles>();
            using (StreamReader reader = new StreamReader(filePathTorrserverDb))
            {
                // Считываем весь файл как одну строку
                string content = reader.ReadToEnd().Trim();

                if (content.StartsWith("{") && content.EndsWith("}"))
                {
                    // Убираем внешние фигурные скобки
                    content = content.Substring(1, content.Length - 2);

                    // Разбиваем строки по запятой
                    var accounts = content.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                   
                    var uniqueLogins = new HashSet<string>(); // Для отслеживания уникальных логинов
                    foreach (var account in accounts)
                    {
                        int keyStartIndex = account.IndexOf("\"") + 1;
                        int keyEndIndex = account.IndexOf("\":");
                        int valueStartIndex = account.IndexOf(":\"") + 2;
                        int valueEndIndex = account.LastIndexOf("\"");

                        if (keyStartIndex >= 0 && keyEndIndex > keyStartIndex &&
                            valueStartIndex > keyEndIndex && valueEndIndex > valueStartIndex)
                        {
                            string login = account.Substring(keyStartIndex, keyEndIndex - keyStartIndex);
                            string password = account.Substring(valueStartIndex, valueEndIndex - valueStartIndex);

                            // Проверяем, есть ли уже такой логин
                            if (!uniqueLogins.Contains(login))
                            {
                                uniqueLogins.Add(login); // Добавляем логин в уникальные
                                profiles.Add(new Profiles
                                {
                                    Login = login,
                                    Password = password,
                                    IsEnabled = true,
                                    UpdatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }
                    return profiles;
                  
                }
            }
            return profiles;
        }
        public static async Task<bool> WriteAllProfilesToConfig(List<Profiles> profiles)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePathTorrserverDb))
                {
                    // Формируем список строк в формате "логин:пароль"
                    var formattedProfiles = new List<string>();
                    foreach (var profile in profiles)
                    {
                        formattedProfiles.Add($"\"{profile.Login}\":\"{profile.Password}\"");
                    }

                    // Оборачиваем в фигурные скобки для сохранения структуры
                    string content = $"{{{string.Join(",", formattedProfiles)}}}";

                    // Записываем все данные в файл
                    await writer.WriteAsync(content);
                }

                return true; // Запись успешно выполнена
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи профилей в конфигурационный файл: {ex.Message}");
            }

            return false; // В случае ошибки
        }

        #endregion OtherProfiles
    }

}
