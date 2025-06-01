using AdTorrBot.BotTelegram.Db;
using AdTorrBotTorrserverBot.Torrserver.ServerArgs;
using AdTorrBotTorrserverBot.Torrserver.BitTor;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using AdTorrBot.BotTelegram.Db.Model;
using AdTorrBotTorrserverBot.Torrserver;
using AdTorrBot.BotTelegram.Db.Model.TorrserverModel;

namespace AdTorrBot.BotTelegram.Handler
{
    public class HandlerCallbackQuery:MessageHandler
    {
        public static async Task HandlerTextInputMessage(Message message, TextInputFlag textInputFlag)
        {
            var text = message.Text;
            var idMessage = message.MessageId;
            var lastTextFlagTrue = textInputFlag.LastTextFlagTrue;

            Console.WriteLine($"Зашел в HandlerTextInputMessage. Последний активный флаг: {lastTextFlagTrue}");

            if (string.IsNullOrEmpty(lastTextFlagTrue))
            {
                Console.WriteLine("Нет активного режима ввода.");
                return;
            }
            await DeleteMessage(idMessage);
            var setTorr = await SqlMethods.GetSettingsTorrProfile(AdminChat);
            var conf = await SqlMethods.GetArgsConfigTorrProfile(AdminChat);
            switch (lastTextFlagTrue)
            {
                #region OtherProfile
                case "FlagNewLoginAndPasswordOtherProfile":
                    Console.WriteLine("Обработка FlagNoteOtherProfile");
                    if (!string.IsNullOrWhiteSpace(text) && text.Contains(":") &&
                         text.Split(":").Length == 2 &&
                         InputTextValidator.ValidateLoginAndPassword(text.Split(":")[0]) &&
                         InputTextValidator.ValidateLoginAndPassword(text.Split(":")[1]))
                    {
                        var login = text.Split(":")[0];
                        var password = text.Split(":")[1];
                        Console.WriteLine($"Новый=> Логин: {login}, Пароль: {password}");
                        if (await SqlMethods.IsLoginExistsAsync(login))
                        {
                            Console.WriteLine($"\r\nЛогин [{login}] уже занят");

                            await botClient.SendTextMessageAsync(AdminChat,
                                $"❗ Вы в режиме создания нового пользователя." +
                                $"\r\nЛогин [{login}] уже занят\r\n" +
                                "Напишите желаемый логин/пароль.\n" +
                                "⚠️ Логин/пароль может содержать только английские буквы и цифры.\r\n" +
                                " Ограничение: 20 символов на логин.\r\n" +
                                " Ограничение: 20 символов на пароль.\r\n" +
                                "между логином и паролем должен быть знак двоеточия :",
                                replyMarkup: KeyboardManager.CreateNewProfileTorrserverUser());
                            break;
                        }
                        else
                        {
                            var newProfile = new Profiles()
                            {
                                Login = login,
                                Password = password,
                                AccessEndDate = DateTime.Now.AddDays(1),
                                IsEnabled = true,
                            };
                            await Task.Delay(1000);
                            await SqlMethods.AddOtherProfileTorrserve(newProfile);
                            Console.WriteLine(newProfile.Login + " добавлен в бд");
                            await SqlMethods.SwitchOffInputFlag();
                            await botClient.SendTextMessageAsync(AdminChat,
                        $"Новый пользователь создан ✅\r\n" +
                        $"🔐 Доступ дан на 24 часа по умолчанию.\r\n" +
                        $"Будет активен после перезагрузки Torrserver.\r\n" +
                        $"/showlogpass_{login}_{password}\r\n" +
                        $"/edit_profile_{newProfile.UniqueId.ToString().Replace("-", "_")}"
                       , replyMarkup: KeyboardManager.GetDeleteThisMessage());
                            break;
                        }

                    }
                    else
                    {
                        Console.WriteLine("Создание нового пользователя не удалось.");

                        await botClient.SendTextMessageAsync(AdminChat,
                              $"❗ Вы в режиме создания нового пользователя." +
                              $"Регистрация нового пользователя не прошла проверку.\r\n" +
                              "Напишите снова желаемый логин/пароль.\n" +
                              "⚠️ Логин/пароль может содержать только английские буквы и цифры.\r\n" +
                              " Ограничение: 20 символов на логин.\r\n" +
                              " Ограничение: 20 символов на пароль.\r\n" +
                              "между логином и паролем должен быть знак двоеточия :",
                              replyMarkup: KeyboardManager.CreateNewProfileTorrserverUser());
                        break;
                    }
                    break;
                case "FlagNoteOtherProfile":
                    Console.WriteLine("Обработка FlagNoteOtherProfile");
                    if (text?.Length > 300)
                    {
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Ограничение 300 символов ! \r\n" +
                            $"У вас: {text.Length} .",
                            replyMarkup: KeyboardManager.ExitEditNoteOtherPfofile());
                    }
                    else
                    {
                        var lastUid = await SqlMethods.GetLastChangeUid();
                        if(!string.IsNullOrEmpty(lastUid))
                        {
                            var profile = await SqlMethods.GetProfileUser(null,lastUid);
                            if(profile is not null)
                            {
                                profile.AdminComment = text;
                                await SqlMethods.EddingProfileUser(profile);
                                await SqlMethods.SwitchOffInputFlag();
                                await botClient.SendTextMessageAsync(AdminChat,
                                $"Заметка сохранена.\u2705" +
                                $"Для профиля:\u2199" +
                                $"/edit_profile_{lastUid.Replace("-","_")}",
                                replyMarkup: KeyboardManager.GetDeleteThisMessage());
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(AdminChat,
                                $"Не удалось сохранить данные (.",
                                replyMarkup: KeyboardManager.ExitEditNoteOtherPfofile());
                        }
                    }
                    break;
                case "FlagLoginPasswordOtherProfile":
                    Console.WriteLine("Обработка TextFlagLoginPasswordOtherProfile");
                    if (!string.IsNullOrWhiteSpace(text) && text.Contains(":") &&
                         text.Split(":").Length == 2 &&
                         InputTextValidator.ValidateLoginAndPassword(text.Split(":")[0]) &&
                         InputTextValidator.ValidateLoginAndPassword(text.Split(":")[1]))
                    {
                        var login = text.Split(":")[0];
                        var password = text.Split(":")[1];
                        Console.WriteLine($"Логин: {login}, Пароль: {password}");
                        if (!await SqlMethods.IsHaveLoginProfileUser(login,true)){
                            Console.WriteLine($"Смена логина/пароля не удалась.\r\nЛогин [{login}] уже занят");

                            await botClient.SendTextMessageAsync(AdminChat,
                                $"❗ Вы в режиме ввода логина/пароля.\r\nЛогин [{login}] уже занят\r\n" +
                                "Напишите желаемый логин/пароль.\n" +
                                "⚠️ Логин/пароль может содержать только английские буквы и цифры.\r\n" +
                                " Ограничение: 20 символов на логин.\r\n" +
                                " Ограничение: 20 символов на пароль.\r\n" +
                                "между логином и паролем должен быть знак двоеточия :",
                                replyMarkup: KeyboardManager.ExitEditLoginPasswordOtherProfile());
                            break;
                        }
                        var uid = await SqlMethods.GetLastChangeUid();
                        var p = await SqlMethods.GetProfileUser(null, uid);
                        await Torrserver.DeleteProfileByLogin(p.Login);
                        p.Login = login;
                        p.Password = password;
                        await SqlMethods.EddingProfileUser(p);
                        await SqlMethods.SwitchOffInputFlag();
                        
                        Console.WriteLine("Смена логина/пароля выполнена.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Вы покинули режим ввода логина/пароля.\r\n" +
                            $"Новый логин ➡️ {login} установлен ✅\r\n" +
                            $"Новый пароль ➡️ {password} установлен ✅\r\n" +
                            $"Для профиля :\r\n" +
                            $"/edit_profile_{uid.Replace("-","_")}\r\n" +
                            $"Изменения вступят после перезапуска Torrserver !",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Смена логина/пароля не удалась.");
                       
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Вы в режиме ввода логина/пароля.\n" +
                            "Напишите желаемый логин/пароль.\n" +
                            "⚠️ Логин/пароль может содержать только английские буквы и цифры.\r\n" +
                            " Ограничение: 20 символов на логин.\r\n" +
                            " Ограничение: 20 символов на пароль.\r\n" +
                            "между логином и паролем должен быть знак двоеточия :",
                            replyMarkup: KeyboardManager.ExitEditLoginPasswordOtherProfile());
                    }
                    break;
                #endregion OtherProfile
                #region User
                case "FlagLogin":
                    Console.WriteLine("Обработка TextInputFlagLogin");
                    if(!await SqlMethods.IsHaveLoginProfileUser(text,false))
                    {
                        Console.WriteLine($"Смена логина не удалась.\r\nЛогин [{text}] уже занят");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"❗ Вы в режиме ввода логина.\r\nЛогин [{text}] уже занят\r\n" +
                            "Напишите желаемый логин.\n" +
                            "⚠️ Логин может содержать только английские буквы и цифры.\r\n Ограничение: 10 символов.",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("Login", 0));
                        break;
                    }
                    if (InputTextValidator.ValidateLoginAndPassword(text))
                    {
                        await AdTorrBotTorrserverBot.Torrserver.Torrserver.ChangeMainAccountTorrserver(text, "", true, false);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagLogin", false);
                        Console.WriteLine("Смена логина выполнена.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Ваш новый логин ➡️ {text} установлен ✅",
                            replyMarkup: KeyboardManager.GetNewLoginPasswordMain());
                    }
                    else
                    {
                        Console.WriteLine("Смена логина не удалась.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Вы в режиме ввода логина.\n" +
                            "Напишите желаемый логин.\n" +
                            "⚠️ Логин может содержать только английские буквы и цифры. Ограничение: 10 символов.",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("Login",0));
                    }
                    break;

                case "FlagPassword":
                    Console.WriteLine("Обработка TextInputFlagPassword");
                    if (InputTextValidator.ValidateLoginAndPassword(text))
                    {
                        await AdTorrBotTorrserverBot.Torrserver.Torrserver.ChangeMainAccountTorrserver("", text, false, true);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagPassword", false);
                        Console.WriteLine("Смена пароля выполнена.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Ваш новый пароль ➡️ {text} установлен ✅",
                            replyMarkup: KeyboardManager.GetNewLoginPasswordMain());
                    }
                    else
                    {
                        Console.WriteLine("Смена пароля не удалась.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Вы в режиме ввода пароля.\n" +
                            "Напишите желаемый пароль.\n" +
                            "⚠️ Пароль может содержать только английские буквы и цифры. Ограничение: 10 символов.",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("Password", 0));
                    }
                    break;
                #endregion User
                #region ServerArgsConfig
                case "FlagServerArgsSettPort":
                    Console.WriteLine("Обработка FlagServerArgsSettPort");
                    if (int.TryParse(text, out int port) && (port == 0 || (port > 1023 && port < 65536)))
                    {
                        conf.Port = port;
                        await ServerArgsConfiguration.WriteConfigArgs(conf);

                        await SqlMethods.SetSettingsServerArgsProfile(conf);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettPort", false);
                        Console.WriteLine($"Веб-порт сервера успешно обновлен: {port}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Веб-порт сервера успешно обновлен ➡️ {port} ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе порта!");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите порт (целое число от 1024 до 65535).\r\n\r\n" +
                            $"Сейчас: {conf.Port}",
                            replyMarkup: KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettPort", (long)conf.Port));
                    }
                    break;

                case "FlagServerArgsSettLogPath":
                    Console.WriteLine("Обработка FlagServerArgsSettLogPath");
                    if (InputTextValidator.IsValidPath(text))
                    {
                        conf.LogPath = text;
                        await ServerArgsConfiguration.WriteConfigArgs(conf);
                        await SqlMethods.SetSettingsServerArgsProfile(conf);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettLogPath", false);
                        Console.WriteLine($"Путь для логов сервера успешно обновлен: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Путь для логов сервера успешно обновлен ➡️\r\n{text} ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе пути для логов!");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите путь для логов сервера (max 4096 символов).\r\n\r\n" +
                            $"Пример: /var/log/server/\r\n" +
                            $"Сейчас: {conf.LogPath}",
                            replyMarkup: KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettLogPath", 0));
                    }
                    break;

                case "FlagServerArgsSettPath":
                    Console.WriteLine("Обработка FlagServerArgsSettPath");
                    if (InputTextValidator.IsValidPath(text))
                    {
                        conf.Path = text;
                        await ServerArgsConfiguration.WriteConfigArgs(conf);
                        await SqlMethods.SetSettingsServerArgsProfile(conf);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettPath", false);
                        Console.WriteLine($"Путь к базе данных и конфигурации успешно обновлен: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Путь к базе данных и конфигурации успешно обновлен ➡️\r\n{text} ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе пути к базе данных!");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите путь к базе данных и конфигурации (max 4096 символов).\r\n\r\n" +
                            $"Пример: /var/lib/database/\r\n" +
                            $"Сейчас: {conf.Path}",
                            replyMarkup: KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettPath", 0));
                    }
                    break;

                case "FlagServerArgsSettSslPort":
                    Console.WriteLine("Обработка FlagServerArgsSettSslPort");
                    if (int.TryParse(text, out int sslPort) && (sslPort == 0 || (sslPort > 1023 && sslPort < 65536)))
                    {
                        conf.SslPort = sslPort;
                        await ServerArgsConfiguration.WriteConfigArgs(conf);
                        await SqlMethods.SetSettingsServerArgsProfile(conf);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettSslPort", false);
                        Console.WriteLine($"SSL порт успешно обновлен: {sslPort}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"SSL порт успешно обновлен ➡️ {sslPort} ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе SSL порта!");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите SSL порт (целое число от 1024 до 65535).\r\n\r\n" +
                            $"Сейчас: {conf.SslPort}",
                            replyMarkup: KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettSslPort", (long)conf.SslPort));
                    }
                    break;

                case "FlagServerArgsSettSslCert":
                    Console.WriteLine("Обработка FlagServerArgsSettSslCert");
                    if (InputTextValidator.IsValidPath(text) && text.EndsWith(".pem", StringComparison.OrdinalIgnoreCase))
                    {
                        conf.SslCert = text;
                        await ServerArgsConfiguration.WriteConfigArgs(conf);
                        await SqlMethods.SetSettingsServerArgsProfile(conf);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettSslCert", false);
                        Console.WriteLine($"Путь к SSL сертификату успешно обновлен: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Путь к SSL сертификату успешно обновлен ➡️\r\n{text} ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе пути к SSL сертификату!");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите путь к SSL сертификату (max 4096 символов).\r\n\r\n" +
                            $"Пример: /etc/letsencrypt/live/domain_name/fullchain.pem\r\n" +
                            $"Сейчас: {conf.SslCert}",
                            replyMarkup: KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettSslCert", 0));
                    }
                    break;

                case "FlagServerArgsSettSslKey":
                    Console.WriteLine("Обработка FlagServerArgsSettSslKey");
                    if (InputTextValidator.IsValidPath(text) && text.EndsWith(".pem", StringComparison.OrdinalIgnoreCase))
                    {
                        conf.SslKey = text;
                        await ServerArgsConfiguration.WriteConfigArgs(conf);
                        await SqlMethods.SetSettingsServerArgsProfile(conf);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettSslKey", false);
                        Console.WriteLine($"Путь к SSL ключу успешно обновлен: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Путь к SSL ключу успешно обновлен ➡️\r\n{text} ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе пути к SSL ключу!");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите путь к SSL ключу (max 4096 символов).\r\n\r\n" +
                            $"Пример: /etc/letsencrypt/live/domain_name/privkey.pem\r\n" +
                            $"Сейчас: {conf.SslKey}",
                            replyMarkup: KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettSslKey", 0));
                    }
                    break;

                case "FlagServerArgsSettWebLogPath":
                    Console.WriteLine("Обработка FlagServerArgsSettWebLogPath");
                    if (InputTextValidator.IsValidPath(text))
                    {
                        conf.WebLogPath = text;
                        await ServerArgsConfiguration.WriteConfigArgs(conf);
                        await SqlMethods.SetSettingsServerArgsProfile(conf);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettWebLogPath", false);
                        Console.WriteLine($"Путь для логов веб-доступа успешно обновлен: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Путь для логов веб-доступа успешно обновлен ➡️\r\n{text} ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе пути для логов веб-доступа!");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите путь для логов веб-доступа (max 4096 символов).\r\n\r\n" +
                            $"Пример: /var/log/web/\r\n" +
                            $"Сейчас: {conf.WebLogPath}",
                            replyMarkup: KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettWebLogPath", 0));
                    }
                    break;

                case "FlagServerArgsSettTorrentsDir":
                    Console.WriteLine("Обработка FlagServerArgsSettTorrentsDir");
                    if (InputTextValidator.IsValidPath(text))
                    {
                        conf.TorrentsDir = text;
                        await ServerArgsConfiguration.WriteConfigArgs(conf);
                        await SqlMethods.SetSettingsServerArgsProfile(conf);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettTorrentsDir", false);
                        Console.WriteLine($"Директория автозагрузки торрентов успешно обновлена: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Директория автозагрузки торрентов успешно обновлена ➡️\r\n{text} ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе директории автозагрузки торрентов!");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите директорию автозагрузки торрентов (max 4096 символов).\r\n\r\n" +
                            $"Пример: /home/user/torrents/\r\n" +
                            $"Сейчас: {conf.TorrentsDir}",
                            replyMarkup: KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettTorrentsDir", 0));
                    }
                    break;

                case "FlagServerArgsSettTorrentAddr":
                    Console.WriteLine("Обработка FlagServerArgsSettTorrentAddr");
                    if (!string.IsNullOrEmpty(text))
                    {
                        conf.TorrentAddr = text;
                        await ServerArgsConfiguration.WriteConfigArgs(conf);
                        await SqlMethods.SetSettingsServerArgsProfile(conf);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettTorrentAddr", false);
                        Console.WriteLine($"Адрес торрент-клиента успешно обновлен: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Адрес торрент-клиента успешно обновлен ➡️\r\n{text} ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе адреса торрент-клиента!");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите адрес торрент-клиента.\r\n\r\n" +
                            $"Пример: http://localhost:8080\r\n" +
                            $"Сейчас: {conf.TorrentAddr}",
                            replyMarkup: KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettTorrentAddr", 0));
                    }
                    break;

                case "FlagServerArgsSettPubIPv4":
                    Console.WriteLine("Обработка FlagServerArgsSettPubIPv4");
                    if (InputTextValidator.IsValidIPv4(text))
                    {
                        conf.PubIPv4 = text;
                        await ServerArgsConfiguration.WriteConfigArgs(conf);
                        await SqlMethods.SetSettingsServerArgsProfile(conf);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettPubIPv4", false);
                        Console.WriteLine($"Публичный IPv4 успешно обновлен: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Публичный IPv4 успешно обновлен ➡️\r\n{text} ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе публичного IPv4!");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите публичный IPv4 адрес.\r\n\r\n" +
                            $"Пример: 192.168.1.1\r\n" +
                            $"Сейчас: {conf.PubIPv4}",
                            replyMarkup: KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettPubIPv4", 0));
                    }
                    break;

                case "FlagServerArgsSettPubIPv6":
                    Console.WriteLine("Обработка FlagServerArgsSettPubIPv6");
                    if (InputTextValidator.IsValidIPv6(text))
                    {
                        conf.PubIPv6 = text;
                        await ServerArgsConfiguration.WriteConfigArgs(conf);
                        await SqlMethods.SetSettingsServerArgsProfile(conf);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettPubIPv6", false);
                        Console.WriteLine($"Публичный IPv6 успешно обновлен: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Публичный IPv6 успешно обновлен ➡️\r\n{text} ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе публичного IPv6!");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите публичный IPv6 адрес.\r\n\r\n" +
                            $"Пример: 2001:0db8:85a3:0000:0000:8a2e:0370:7334\r\n" +
                            $"Сейчас: {conf.PubIPv6}",
                            replyMarkup: KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettPubIPv6", 0));
                    }
                    break;
                #endregion ServerArgsConfig
                #region BittorrConfig
                case "FlagTorrSettCacheSize":
                    Console.WriteLine("Обработка FlagTorrSettCacheSize");
                    if (int.TryParse(text, out int cacheSize) && (cacheSize >31&&cacheSize<257))
                    {
                        setTorr.CacheSize = cacheSize;
                        await BitTorrConfigation.WriteConfig(setTorr);
                        await SqlMethods.SetSettingsTorrProfile(setTorr); 
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettCacheSize", false);
                        Console.WriteLine($"Размер кэша успешно обновлен: {cacheSize} MB.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Размер кэша успешно обновлен ➡️ {cacheSize} MB ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе размера кэша.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите размер кэша в MB (целое число от 32 до 256).\r\n\r\n" +
                            $"Сейчас {setTorr.CacheSize} MB",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettCacheSize", setTorr.CacheSize));
                    }
                    break;
                // Шаблон для остальных флагов
                case "FlagTorrSettReaderReadAHead":
                    Console.WriteLine("FlagTorrSettReaderReadAHead");
                    if (int.TryParse(text, out int readHead) && (readHead > 4 && readHead < 101))
                    {
                        setTorr.ReaderReadAHead = readHead;
                        await BitTorrConfigation.WriteConfig(setTorr);
                        await SqlMethods.SetSettingsTorrProfile(setTorr); 
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettReaderReadAHead", false);
                        Console.WriteLine($"Опережающий кэш обновлен: {readHead} MB.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Опережающий кэш успешно обновлен ➡️ {readHead} % ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе опережающего кэша.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите опережающий кэш (целое число от 5 до 100).\r\n\r\n" +
                            $"Сейчас {setTorr.ReaderReadAHead} %",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettReaderReadAHead", setTorr.ReaderReadAHead));
                    }
                    break;
                case "FlagTorrSettPreloadCache":
                    Console.WriteLine("FlagTorrSettPreloadCache");
                    if (int.TryParse(text, out int preLoadCache) && (preLoadCache > 4 && preLoadCache < 101))
                    {
                        setTorr.ReaderReadAHead = preLoadCache;
                        await BitTorrConfigation.WriteConfig(setTorr);
                        await SqlMethods.SetSettingsTorrProfile(setTorr);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettPreloadCache", false);
                        Console.WriteLine($"Буфер предзагрузки обновлен: {preLoadCache} %.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Буфер предзагрузки успешно обновлен ➡️ {preLoadCache} % ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе буфера предзагрузки.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите буфер предзагрузки (целое число от 5 до 100).\r\n\r\n" +
                            $"Сейчас {setTorr.PreloadCache} %",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettPreloadCache", setTorr.PreloadCache));
                    }
                    break;
                case "FlagTorrSettTorrentDisconnectTimeout":
                    Console.WriteLine("FlagTorrSettTorrentDisconnectTimeout");
                    if (int.TryParse(text, out int torrentDisconnectTimeout) && (torrentDisconnectTimeout > 0))
                    {
                        setTorr.TorrentDisconnectTimeout = torrentDisconnectTimeout;
                        await BitTorrConfigation.WriteConfig(setTorr);
                        await SqlMethods.SetSettingsTorrProfile(setTorr);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettTorrentDisconnectTimeout", false);
                        Console.WriteLine($"Тайм-аут отключения торрента обновлен: {torrentDisconnectTimeout} %.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Тайм-аут отключения торрента успешно обновлен ➡️ {torrentDisconnectTimeout} сек. ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе буфера предзагрузки.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите тайм-аут отключения торрента (целое число от 1).\r\n\r\n" +
                            $"Сейчас {setTorr.PreloadCache} сек.",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettTorrentDisconnectTimeout", setTorr.TorrentDisconnectTimeout));
                    }
                    break;
                case "FlagTorrSettConnectionsLimit":
                    Console.WriteLine("FlagTorrSettConnectionsLimit");
                    if (int.TryParse(text, out int connectionsLimit) && (connectionsLimit > 0))
                    {
                        setTorr.ConnectionsLimit = connectionsLimit;
                        await BitTorrConfigation.WriteConfig(setTorr);
                        await SqlMethods.SetSettingsTorrProfile(setTorr);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettConnectionsLimit", false);
                        Console.WriteLine($"Торрент соединения обновлены: {connectionsLimit} шт.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Торрент соединения успешно обновлены ➡️ {connectionsLimit} шт. ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе торрент соединений.");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите кол-во торрент соединений (целое число от 0).\r\n\r\n" +
                            $"Сейчас {setTorr.ConnectionsLimit} сек.",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettConnectionsLimit", setTorr.ConnectionsLimit));
                    }
                    break;
                case "FlagTorrSettDownloadRateLimit":
                    Console.WriteLine("FlagTorrSettDownloadRateLimit");
                    if (int.TryParse(text, out int downloadRateLimit) && (downloadRateLimit > 0))
                    {
                        setTorr.DownloadRateLimit = downloadRateLimit;
                        await BitTorrConfigation.WriteConfig(setTorr);
                        await SqlMethods.SetSettingsTorrProfile(setTorr);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettDownloadRateLimit", false);
                        Console.WriteLine($"Ограничение скорости загрузки обновлены: {downloadRateLimit} мб/сек");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Ограничение скорости загрузки успешно обновлены ➡️ {downloadRateLimit} мб/сек ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе ограничение скорости загрузки мб/сек");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите ограничение скорости загрузки в мб/сек (целое число от 0).\r\n\r\n" +
                            $"Сейчас {setTorr.DownloadRateLimit} мб/сек",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettDownloadRateLimit", setTorr.DownloadRateLimit));
                    }
                    break;
                case "FlagTorrSettUploadRateLimit":
                    Console.WriteLine("FlagTorrSettUploadRateLimit");
                    if (int.TryParse(text, out int uploadRateLimit) && (uploadRateLimit > 0))
                    {
                        setTorr.UploadRateLimit = uploadRateLimit;
                        await BitTorrConfigation.WriteConfig(setTorr);
                        await SqlMethods.SetSettingsTorrProfile(setTorr);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettUploadRateLimit", false);
                        Console.WriteLine($"Ограничение скорости отдачи обновлены: {uploadRateLimit} мб/сек");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Ограничение скорости отдачи успешно обновлены ➡️ {uploadRateLimit} мб/сек ✅",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе ограничение скорости отдачи мб/сек");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите ограничение скорости отдачи в мб/сек (целое число от 0).\r\n\r\n" +
                            $"Сейчас {setTorr.UploadRateLimit} мб/сек",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettUploadRateLimit", setTorr.UploadRateLimit));
                    }
                    break;
                case "FlagTorrSettPeersListenPort":
                    Console.WriteLine("FlagTorrSettPeersListenPort");
                    if (int.TryParse(text, out int peersListenPort) &&
                        (peersListenPort == 0 || (peersListenPort > 1023 && peersListenPort < 65536)))
                    {
                        if ( ServerManagement.ServerInfo.IsPortAvailable(peersListenPort))
                        {
                            setTorr.PeersListenPort = peersListenPort;
                            await BitTorrConfigation.WriteConfig(setTorr);
                            await SqlMethods.SetSettingsTorrProfile(setTorr);
                            await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettPeersListenPort", false);
                            Console.WriteLine($"Порт успешно обновлен: {peersListenPort} ");
                            await botClient.SendTextMessageAsync(AdminChat,
                                $"Порт успешно обновлен ➡️ {peersListenPort} ✅",
                                replyMarkup: KeyboardManager.GetDeleteThisMessage());
                        }
                        else
                        {
                            Console.WriteLine("Ошибка при вводе порта (порт занят)!");
                            await botClient.SendTextMessageAsync(AdminChat,
                                "❗ Неверный ввод (порт занят).\n" +
                                "Введите порт (целое число от 1024 до 65535).\r\n\r\n" +
                                $"Сейчас {setTorr.UploadRateLimit}",
                                replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettPeersListenPort", setTorr.PeersListenPort));
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе порта !");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите порт (целое число от 1024 до 65535).\r\n\r\n" +
                            $"Сейчас {setTorr.PeersListenPort}",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettPeersListenPort", setTorr.PeersListenPort));
                    }
                    break;
                case "FlagTorrSettFriendlyName":
                    Console.WriteLine("FlagTorrSettFriendlyName");
                    if (text?.Length<31)
                    {
                        setTorr.FriendlyName = text;
                        if (text == "0")
                        {
                            setTorr.FriendlyName = "";
                        }
                        await BitTorrConfigation.WriteConfig(setTorr);
                        await SqlMethods.SetSettingsTorrProfile(setTorr);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettFriendlyName", false);
                        Console.WriteLine($"Имя сервера DLNA обновлено: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Имя сервера DLNA успешно обновлено:  {text}",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе имени сервера DLNA !");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите имя сервера DLNA (max 30 символов).\r\n\r\n" +
                            $"Сейчас: {setTorr.FriendlyName}",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettFriendlyName", 0));
                    }
                    break;
                case "FlagTorrSettRetrackersMode":
                    Console.WriteLine("FlagTorrSettRetrackersMode");
                    if ((int.TryParse(text, out int retrackersMode) && retrackersMode >= 0 && retrackersMode < 4))
                    {
                        setTorr.RetrackersMode = retrackersMode;
                        
                        await BitTorrConfigation.WriteConfig(setTorr);
                        await SqlMethods.SetSettingsTorrProfile(setTorr);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettFriendlyName", false);
                        Console.WriteLine($"Режим ретрекеров обновлен: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Режим ретрекеров успешно обновлен:  {text}",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе режима ретрекеров!");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите число (0-3).\r\n\r\n" +
                            $"Сейчас: {setTorr.RetrackersMode}\r\n" +
                            "0-ничего не делать\r\n"+
                            "1-добавлять (по умолчанию)\r\n"+
                            "2-удалять\r\n"+
                            "3-заменять\r\n",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettRetrackersMode", setTorr.RetrackersMode));
                    }
                    break;
                case "FlagTorrSettSslPort":
                    Console.WriteLine("FlagTorrSettSslPort");
                    if (int.TryParse(text, out int newSslPort) &&
                        (newSslPort == 0 || (newSslPort > 1023 && newSslPort < 65536)))
                    {
                        if (ServerManagement.ServerInfo.IsPortAvailable(newSslPort))
                        {
                            setTorr.SslPort = newSslPort;
                            await BitTorrConfigation.WriteConfig(setTorr);
                            await SqlMethods.SetSettingsTorrProfile(setTorr);
                            await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettSslPort", false);
                            Console.WriteLine($"Порт успешно обновлен: {newSslPort} ");
                            await botClient.SendTextMessageAsync(AdminChat,
                                $"Порт ssl успешно обновлен ➡️ {newSslPort} ✅",
                                replyMarkup: KeyboardManager.GetDeleteThisMessage());
                        }
                        else
                        {
                            Console.WriteLine("Ошибка при вводе порта ssl (порт занят)!");
                            await botClient.SendTextMessageAsync(AdminChat,
                                "❗ Неверный ввод (порт занят).\n" +
                                "Введите порт ssl (целое число от 1024 до 65535).\r\n\r\n" +
                                "0 - (8091)по умолчанию.\r\n" +
                                $"Сейчас {setTorr.SslPort}",
                                replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettSslPort", setTorr.SslPort));
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при вводе порта ssl !");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\n" +
                            "Введите порт ssl (целое число от 1024 до 65535).\r\n\r\n" +
                            "0 - (8091)по умолчанию.\r\n" +
                            $"Сейчас {setTorr.SslPort}",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettSslPort", setTorr.SslPort));
                    }
                    break;
                case "FlagTorrSettSslCert":
                    Console.WriteLine("FlagTorrSettSslCert");
                    if (InputTextValidator.IsValidPath(text)&&text.EndsWith(".pem",StringComparison.OrdinalIgnoreCase))
                    {
                        setTorr.SslCert = text;
                        await BitTorrConfigation.WriteConfig(setTorr);
                        await SqlMethods.SetSettingsTorrProfile(setTorr);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettSslCert", false);
                        Console.WriteLine($"Путь SSL сертификата обновлен: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Путь SSL сертификата успешно обновлен:  \r\n" +
                            $"{text}",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {

                        Console.WriteLine("Ошибка при вводе пути SSL сертификата !");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\r\n" +
                            "Введите путь SSL сертификата (max 4096 символов).\r\n\r\n" +
                            $"Пример: /etc/letsencrypt/live/domain_name/fullchain.pem\r\n" +
                            $"Сейчас: {setTorr.SslCert}",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettSslCert", 0));
                    }
                    break;
                case "FlagTorrSettSslKey":
                    Console.WriteLine("FlagTorrSettSslKey");
                    if (InputTextValidator.IsValidPath(text) && text.EndsWith(".pem", StringComparison.OrdinalIgnoreCase))
                    {
                        setTorr.SslKey = text;
                        await BitTorrConfigation.WriteConfig(setTorr);
                        await SqlMethods.SetSettingsTorrProfile(setTorr);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettSslKey", false);
                        Console.WriteLine($"Путь SSL ключа обновлен: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Путь SSL ключа успешно обновлен:  \r\n" +
                            $"{text}",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {

                        Console.WriteLine("Ошибка при вводе пути SSL сертификата !");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\r\n" +
                            "Введите путь SSL ключа (max 4096 символов).\r\n\r\n" +
                            $"Пример: /etc/letsencrypt/live/доменное_имя/privkey.pem\r\n" +
                            $"Сейчас: {setTorr.SslKey}",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettSslKey", 0));
                    }
                    break;
                case "FlagTorrSettTorrentsSavePath":
                    Console.WriteLine("FlagTorrSettTorrentsSavePath");
                    if (InputTextValidator.IsValidPath(text))
                    {
                        setTorr.TorrentsSavePath = text;
                        await BitTorrConfigation.WriteConfig(setTorr);
                        await SqlMethods.SetSettingsTorrProfile(setTorr);
                        await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettTorrentsSavePath", false);
                        Console.WriteLine($"Путь сохр. торрентов обновлен: {text}");
                        await botClient.SendTextMessageAsync(AdminChat,
                            $"Путь сохр. торрентов успешно обновлен:  \r\n" +
                            $"{text}",
                            replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    }
                    else
                    {
                        
                        Console.WriteLine("Ошибка при вводе пути сохр. торрентов !");
                        await botClient.SendTextMessageAsync(AdminChat,
                            "❗ Неверный ввод.\r\n" +
                            "Введите путь сохр. торрентов (max 4096 символов).\r\n\r\n" +
                            $"Пример: /home/user/Documents\r\n" +
                            $"Сейчас: {setTorr.TorrentsSavePath}",
                            replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettTorrentsSavePath", 0));
                    }
                    break;
                # endregion BittorrConfig
                default:
                    Console.WriteLine($"Обработка для {lastTextFlagTrue} пока не реализована.");
                    await SqlMethods.SwitchOffInputFlag();
                    await botClient.SendTextMessageAsync(AdminChat,
                        $"Вы покинули режим ввода данных: {lastTextFlagTrue}. данный режим ввода нереализован.",
                       
                        replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    break;
            }
        }

        public static async Task CheckSettingServerArgsAndExecute(CallbackQuery callbackQuery, string inputSetting)
        {
            Console.WriteLine($"Пришло в CheckSettingServerArgsAndExecute:{inputSetting}");
            var callbackData = callbackQuery.Data;
            var idMessage = callbackQuery.Message.MessageId;
            var conf = await ServerArgsConfiguration.ReadConfigArgs();
            string setting = inputSetting.Split("torrConfigSetOne")[1].ToLower();
            int value = int.Parse(inputSetting.Split("torrConfigSetOne")[0]);
            string enabledSymbol = "\u2705";  // ✅
            string disabledSymbol = "\u274C"; // ❌
            string confName = "Конфиг Torrserver";

            switch (setting)
            {
                #region Обработка кнопок Конфига Torrserver (bool кнопки)
                // Поля типа bool - переключение на противоположное состояние
                case "httpauth":
                    conf.HttpAuth = !conf.HttpAuth;
                    await SendOrEditMessage(idMessage, $"HTTP-аутентификация теперь {(conf.HttpAuth ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWServerArgsConfig(), confName);
                    break;

                case "readonlymode":
                    conf.ReadOnlyMode = !conf.ReadOnlyMode;
                    await SendOrEditMessage(idMessage, $"Режим только для чтения теперь {(conf.ReadOnlyMode ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWServerArgsConfig(), confName);
                    break;

                case "ssl":
                    conf.Ssl = !conf.Ssl;
                    await SendOrEditMessage(idMessage, $"HTTPS теперь {(conf.Ssl ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWServerArgsConfig(), confName);
                    break;

                case "dontkill":
                    conf.DontKill = !conf.DontKill;
                    await SendOrEditMessage(idMessage, $"Запрет завершения сервера теперь {(conf.DontKill ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWServerArgsConfig(), confName);
                    break;

                case "ui":
                    conf.Ui = !conf.Ui;
                    await SendOrEditMessage(idMessage, $"Интерфейс в браузере теперь {(conf.Ui ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWServerArgsConfig(), confName);
                    break;

                case "searchwa":
                    conf.SearchWa = !conf.SearchWa;
                    await SendOrEditMessage(idMessage, $"Поиск без аутентификации теперь {(conf.SearchWa ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWServerArgsConfig(), confName);
                    break;

                case "help":
                    conf.Help = !conf.Help;
                    await SendOrEditMessage(idMessage, $"Справка теперь {(conf.Help ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWServerArgsConfig(), confName);
                    break;

                case "version":
                    conf.Version = !conf.Version;
                    await SendOrEditMessage(idMessage, $"Версия программы теперь {(conf.Version ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWServerArgsConfig(), confName);
                    break;
                #endregion Обработка кнопок Конфига Torrserver (bool кнопки)

                #region  Обработка кнопок Конфига Torrserver (кнопки с вводом)
                // Поля, требующие ввода данных (int, long, string)

                case "port":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettPort", true);
                    if (value != 0) 
                    {
                        conf.Port = value;
                    }
                    
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода веб-порта сервера. Пожалуйста, введите новый порт.\r\n" +
                        $"Сейчас: {conf.Port}", KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettPort", (long)conf.Port), confName);
                    break;

                case "logpath":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettLogPath", true);
                    if (value == 1)
                    {
                        conf.LogPath = "/opt/torrserver/torrserver.log";
                    }
                    
                  
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода пути для логов сервера. Пожалуйста, введите новый путь.\r\n" +
                        $"Сейчас: {conf.LogPath}", KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettLogPath", 0), confName);
                    break;

                case "path":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettPath", true);
                    if (value == 1)
                    {
                        conf.Path = "/opt/torrserver";
                    }
                    
                   
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода пути к базе данных и конфигурации. Пожалуйста, введите новый путь.\r\n" +
                        $"Сейчас: {conf.Path}", KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettPath", 0), confName);
                    break;

                case "sslport":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettSslPort", true);
                    if (value != 0)
                    {
                        conf.SslPort = value;
                    }
                    
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода HTTPS порта. Пожалуйста, введите новый порт.\r\n" +
                        $"Сейчас: {conf.SslPort}", KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettSslPort", (long)conf.SslPort), confName);
                    break;

                case "sslcert":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettSslCert", true);
                    if (value == 1)
                    {
                        conf.SslCert = null;
                    }
                   
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода пути к SSL-сертификату. Пожалуйста, введите новый путь.\r\n" +
                        $"Сейчас: {conf.SslCert}", KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettSslCert", 0), confName);
                    break;

                case "sslkey":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettSslKey", true);
                    if (value == 1)
                    {
                        conf.SslKey = null;
                    }
                    
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода пути к SSL-ключу. Пожалуйста, введите новый путь.\r\n" +
                        $"Сейчас: {conf.SslKey}", KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettSslKey", 0), confName);
                    break;

                case "weblogpath":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettWebLogPath", true);
                    if (value == 1)
                    {
                        conf.WebLogPath = null;
                    }
                  
                   
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода пути для логов веб-доступа. Пожалуйста, введите новый путь.\r\n" +
                        $"Сейчас: {conf.WebLogPath}", KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettWebLogPath", 0), confName);
                    break;

                case "torrentsdir":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettTorrentsDir", true);
                    if (value == 1)
                    {
                        conf.TorrentsDir = null;
                    }
                    
                    
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода директории автозагрузки торрентов. Пожалуйста, введите новую директорию.\r\n" +
                        $"Сейчас: {conf.TorrentsDir}", KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettTorrentsDir", 0), confName);
                    break;

                case "torrentaddr":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettTorrentAddr", true);
                    if (value == 1)
                    {
                        conf.TorrentAddr = null;
                    }
                   
                    
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода адреса торрент-клиента. Пожалуйста, введите новый адрес.\r\n" +
                        $"Сейчас: {conf.TorrentAddr}", KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettTorrentAddr", 0), confName);
                    break;

                case "pubipv4":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettPubIPv4", true);
                    if (value == 1)
                    {
                        conf.PubIPv4 = null;
                    }
                   
                    
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода публичного IPv4. Пожалуйста, введите новый IPv4 адрес.\r\n" +
                        $"Сейчас: {conf.PubIPv4}", KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettPubIPv4", 0), confName);
                    break;

                case "pubipv6":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagServerArgsSettPubIPv6", true);
                    if (value == 1)
                    {
                        conf.PubIPv6 = null;
                    }
                  
                    
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода публичного IPv6. Пожалуйста, введите новый IPv6 адрес.\r\n" +
                        $"Сейчас: {conf.PubIPv6}", KeyboardManager.CreateExitServerArgsConfigInputButton("ServerArgsSettPubIPv6", 0), confName);
                    break;
                #endregion Обработка кнопок Конфига Torrserver (кнопки с вводом)
                default:
                    await SendOrEditMessage(idMessage, "Неизвестная настройка", KeyboardManager.buttonHideButtots, confName);
                    break;
            }
           await  ServerArgsConfiguration.WriteConfigArgs(conf);
            return ;
        }
            public static async Task CheckSettingAndExecute(CallbackQuery callbackQuery, string inputSetting)
        {
            Console.WriteLine($"Пришло в CheckSettingsAndExecute:{inputSetting}");
            var callbackData = callbackQuery.Data;
            var idMessage = callbackQuery.Message.MessageId;
            var conf = await BitTorrConfigation.ReadConfig();
            string setting = inputSetting.Split("torrSetOne")[1].ToLower();
            
            int value = int.Parse(inputSetting.Split("torrSetOne")[0]);
            string enabledSymbol = "\u2705";  // ✅
            string disabledSymbol = "\u274C"; // ❌
            string confName = "Настройки Torrserver";
            switch (setting)
            {
                // Поля типа bool - переключение на противоположное состояние
                case "usedisk":
                    conf.UseDisk = !conf.UseDisk;
                    await SendOrEditMessage(idMessage, $"Использование диска теперь {(conf.UseDisk ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "enableipv6":
                    conf.EnableIPv6 = !conf.EnableIPv6;
                    await SendOrEditMessage(idMessage, $"IPv6 теперь {(conf.EnableIPv6 ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "disablestcp":
                    conf.DisableTCP = !conf.DisableTCP;
                    await SendOrEditMessage(idMessage, $"Отключение TCP теперь {(conf.DisableTCP ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "disableutp":
                    conf.DisableUTP = !conf.DisableUTP;
                    await SendOrEditMessage(idMessage, $"μTP теперь {(conf.DisableUTP ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "disablepex":
                    conf.DisablePEX = !conf.DisablePEX;
                    await SendOrEditMessage(idMessage, $"PEX теперь {(conf.DisablePEX ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "forceencrypt":
                    conf.ForceEncrypt = !conf.ForceEncrypt;
                    await SendOrEditMessage(idMessage, $"Принудительное шифрование теперь {(conf.ForceEncrypt ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "disabledht":
                    conf.DisableDHT = !conf.DisableDHT;
                    await SendOrEditMessage(idMessage, $"DHT теперь {(conf.DisableDHT ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "disableupnp":
                    conf.DisableUPNP = !conf.DisableUPNP;
                    await SendOrEditMessage(idMessage, $"UPNP теперь {(conf.DisableUPNP ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "enabledlna":
                    conf.EnableDLNA = !conf.EnableDLNA;
                    await SendOrEditMessage(idMessage, $"DLNA теперь {(conf.EnableDLNA ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "enablerutorsearch":
                    conf.EnableRutorSearch = !conf.EnableRutorSearch;
                    await SendOrEditMessage(idMessage, $"Поиск по RuTor теперь {(conf.EnableRutorSearch ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "enabledebug":
                    conf.EnableDebug = !conf.EnableDebug;
                    await SendOrEditMessage(idMessage, $"Режим отладки теперь {(conf.EnableDebug ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "responsivemode":
                    conf.ResponsiveMode = !conf.ResponsiveMode;
                    await SendOrEditMessage(idMessage, $"Быстрый режим чтения теперь {(conf.ResponsiveMode ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "disableupload":
                    conf.DisableUpload = !conf.DisableUpload;
                    await SendOrEditMessage(idMessage, $"Отключение отдачи теперь {(conf.DisableUpload ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                case "removecacheondrop":
                    conf.RemoveCacheOnDrop = !conf.RemoveCacheOnDrop;
                    await SendOrEditMessage(idMessage, $"Удаление кеша при сбросе теперь {(conf.RemoveCacheOnDrop ? enabledSymbol : disabledSymbol)}", KeyboardManager.GetShoWBitTorrConfig(), confName);
                    break;

                // Поля, требующие ввода данных (int, long, string)

                case "cachesize":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettCacheSize", true);

                    conf.CacheSize = value;
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода размера кеша. Пожалуйста, введите новое значение (MB).\r\n" +
                        "Min 32MB - Max 256MB\r\n\r\n" +
                        $"Сейчас: {conf.CacheSize} МБ", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettCacheSize", conf.CacheSize), confName);
                    break;

                case "readerreadahead":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettReaderReadAHead", true);
                    conf.ReaderReadAHead = value;
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода значения для опережающего кеша. Пожалуйста, введите новое значение.\r\n" +
                        "Min 5% - Max 100%\r\n\r\n" +
                        $"Сейчас: {conf.ReaderReadAHead} %", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettReaderReadAHead", conf.ReaderReadAHead), confName);
                    break;

                case "preloadcache":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettPreloadCache", true);
                    conf.PreloadCache = value;
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода размера буфера предзагрузки. Пожалуйста, введите новое значение.\r\n" +
                        "Min 5% - Max 100%\r\n\r\n" +
                        $"Сейчас: {conf.PreloadCache} %", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettPreloadCache", conf.PreloadCache), confName);
                    break;

                case "torrentdisconnecttimeout":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettTorrentDisconnectTimeout", true);
                    conf.TorrentDisconnectTimeout = value;
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода тайм-аута отключения торрентов. Пожалуйста, введите новое значение (в секундах).\r\n" +
                        $"Сейчас: {conf.TorrentDisconnectTimeout} сек.", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettTorrentDisconnectTimeout", conf.TorrentDisconnectTimeout), confName);
                    break;

                case "connectionslimit":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettConnectionsLimit", true);
                    conf.ConnectionsLimit = value;
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода лимита соединений для торрентов. Пожалуйста, введите новое значение.\r\n" +
                        $"Сейчас: {conf.ConnectionsLimit} соед. ", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettConnectionsLimit", conf.ConnectionsLimit), confName);
                    break;

                case "downloadratelimit":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettDownloadRateLimit", true);
                    conf.DownloadRateLimit = value;
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода ограничения скорости загрузки. Пожалуйста, введите новое значение (мб/с).\r\n" +
                        $"Сейчас: {conf.DownloadRateLimit} мб/сек", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettDownloadRateLimit", conf.DownloadRateLimit), confName);
                    break;

                case "uploadratelimit":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettUploadRateLimit", true);
                    conf.UploadRateLimit = value;
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода ограничения скорости отдачи. Пожалуйста, введите новое значение (мб/с).\r\n" +
                        $"Сейчас: {conf.UploadRateLimit} мб/сек", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettUploadRateLimit", conf.UploadRateLimit), confName);
                    break;

                case "peerslistenport":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettPeersListenPort", true);
                    conf.PeersListenPort = value;
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода порта для входящих подключений. Пожалуйста, введите новый порт.\r\n" +
                        $"Порт должен быть в диапазоне от 1024 до 65535.\r\n" +
                        $"Сейчас: {conf.PeersListenPort} порт", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettPeersListenPort", conf.PeersListenPort), confName);
                    break;

                case "retrackersmode":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettRetrackersMode", true);
                    conf.RetrackersMode = value;
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода режима ретрекеров. Пожалуйста, введите новое значение (целое число 0-3).\r\n" +
                        "0 - ничего не делать\r\n" +
                        "1 - добавлять (по умолчанию)\r\n" +
                        "2 - удалять\r\n" +
                        "3 - заменять\r\n" +
                        $"Сейчас: {conf.RetrackersMode} режим", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettRetrackersMode", conf.RetrackersMode), confName);
                    break;

                case "sslport":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettSslPort", true);
                    conf.SslPort = value;
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода SSL порта. Пожалуйста, введите новый порт.\r\n" +
                        "0 - (8091)по умолчанию.\r\n" +
                        $"Сейчас: {conf.SslPort} порт", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettSslPort", conf.SslPort), confName);
                    break;

                case "friendlyname":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettFriendlyName", true);
                    if (value == 1)
                    {
                        conf.FriendlyName = "";
                    }

                    await SendOrEditMessage(idMessage, "Вы в режиме ввода имени сервера DLNA. Пожалуйста, введите новое имя.\r\n" +
                        "Ограничение 30 символов\r\n" +
                        $"Сейчас: {conf.FriendlyName} .", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettFriendlyName", 0), confName);
                    break;

                case "torrentssavepath":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettTorrentsSavePath", true);

                    if (value == 1)
                    {
                        conf.TorrentsSavePath = "";
                    }
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода пути для сохранения торрентов. \r\n" +
                       "Введите путь сохр. торрентов (max 4096 символов).\r\n\r\n" +
                       $"Пример: /home/user/Documents\r\n" +
                        $"Сейчас: {conf.TorrentsSavePath}", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettTorrentsSavePath", 0), confName);
                    break;

                case "sslcert":
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettSslCert", true);
                            if(value == 1)
                    {
                        conf.SslCert = "";
                    }
                            await SendOrEditMessage(idMessage, "Вы в режиме ввода пути к SSL сертификату.\r\n" +
                                "Пожалуйста, введите путь (max 4096 символов).\r\n" +
                                  $"Пример: /etc/letsencrypt/live/domain_name/fullchain.pem\r\n" +
                                $"Сейчас: {conf.SslCert} ", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettSslCert", 0), confName);
                            break;

                case "sslkey":
                            await SqlMethods.SwitchTorSettingsInputFlag("FlagTorrSettSslKey", true);
                    if (value == 1)
                    {
                        conf.SslKey = "";
                    }
                    await SendOrEditMessage(idMessage, "Вы в режиме ввода пути к SSL ключу.\r\n" +
                                "Пожалуйста, введите путь (max 4096 символов).\r\n" +
                                "Пример: /etc/letsencrypt/live/доменное_имя/privkey.pem\r\n" +
                                $"Сейчас: {conf.SslKey}", KeyboardManager.CreateExitBitTorrConfigInputButton("TorrSettSslKey",0), confName);
                            break;

                 default:
                            await SendOrEditMessage(idMessage, "Неизвестная настройка", KeyboardManager.buttonHideButtots, confName);
                            break;
                    }


                    await BitTorrConfigation.WriteConfig(conf);
         
        }

        public static async Task SendOrEditMessage(int idMessage, string message, InlineKeyboardMarkup keyCallback,string confName)
        {
            try
            {
                await botClient.EditMessageTextAsync(
                    AdminChat,
                    idMessage,
                    $"\u2699 {confName}\r\n" + message,
                    replyMarkup: keyCallback
                );
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex) when (ex.Message.Contains("message is not modified"))
            {
                Console.WriteLine($"Сообщение {idMessage} не изменено, так как текст идентичен текущему.");
            }
          
        }



    }
}
