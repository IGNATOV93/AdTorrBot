using AdTorrBotTorrserverBot.BotTelegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using AdTorrBotTorrserverBot.Torrserver;
using AdTorrBot.BotTelegram.Db;
using AdTorrBot.BotTelegram.Db.Model;
using AdTorrBot.ServerManagement;
using AdTorrBotTorrserverBot.Torrserver.BitTor;
using AdTorrBotTorrserverBot.Torrserver.ServerArgs;
using Microsoft.EntityFrameworkCore;



namespace AdTorrBot.BotTelegram.Handler
{
    public abstract class MessageHandler
    {
        protected static TelegramBotClient botClient = TelegramBot.client;
        protected readonly static string AdminChat = TelegramBot.AdminChat;
        public static async Task HandleUpdate(Update update)
        {
            if (update.Type == UpdateType.Message)
            {
                Console.WriteLine($"Пришло text сообщение: {update?.Message?.Text}");
                var textInputFlags = await SqlMethods.GetTextInputFlag();
                if (textInputFlags.CheckAllBooleanFlags())
                {
                    await HandlerCallbackQuery.HandlerTextInputMessage(update.Message, textInputFlags);
                    return;
                }

                await HandlerTextButtonAndCommandMessage(update.Message);

                return;
            }
            if (update.Type == UpdateType.CallbackQuery)
            {
                Console.WriteLine($"Пришло Callback сообщение: {update.CallbackQuery.Data}");
                await HandlerCallbackQueryCommands(update.CallbackQuery);
                return;
            }
            return;
        }

        public static async Task DeleteMessage(int messageId)
        {
            try
            {
                await botClient.DeleteMessageAsync(AdminChat, messageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return;
        }
   
        private static async Task HandlerTextButtonAndCommandMessage(Message message)
        {
            var text = message.Text;
            var idMessage = message.MessageId;
            if(text == null) { return; }
            #region Обработка команд
            if (text == "/start")
            {
                await DeleteMessage(idMessage);
                await botClient.SendTextMessageAsync(AdminChat
                                                          , "Бот по управлению Torrserver приветствует тебя !"
                                                          , replyMarkup: KeyboardManager.GetMainKeyboard());
                return;
            }
            #endregion Обработка команд
            #region Обработка текстовых кнопок
            #region OtherProfile
            
            if (text.Contains("/showlogpass_"))
            {
                var lp= text.Split("/showlogpass_")[1]?.Replace("_",":");
                var log = lp.Split(":")[0];
                var pass= lp.Split(":")[1];
                var Profile = await SqlMethods.FindProfileToLoginAndPassword(log, pass);
                if (Profile == null) 
                {
                    await botClient.SendTextMessageAsync(AdminChat, "Данный профиль не найден в базе данных."
                      , replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    return;
                }
                else
                {
                    var endTime = Profile.AccessEndDate.HasValue ? Profile.AccessEndDate.Value.ToString("dd.MM.yyyy HH:mm") : "Не задано";
                    var remainingTime = Profile.AccessEndDate.HasValue
                                         ? Profile.AccessEndDate.Value - DateTime.UtcNow
                                         : (TimeSpan?)null;
                    var haveTime = "🕒 Осталось: Не задано или доступ истёк\r\n";
                    if (remainingTime.HasValue && remainingTime.Value.TotalMilliseconds > 0)
                    {
                        haveTime= $"🕒 Осталось: {remainingTime.Value.Days} дн. {remainingTime.Value.Hours} ч.";
                    }

                   
                    var сonfigArgs =await ServerArgsConfiguration.ReadConfigArgs();
                    var (protocol, port) = ServerArgsConfiguration.GetProtocolAndPort(сonfigArgs);
                    string ip = ServerControl.GetPublicIp();
                    string url = $"{protocol}://{ip}:{port}";

                    var result = $"👤 Логин: {Profile.Login}\r\n" +
                                 $"🗝️ Пароль: {Profile.Password}\r\n\r\n" +
                                 $"⏳ Окончание доступа : {endTime}\r\n" +
                                 $"{haveTime}\r\n\r\n"+
                                 $"В формате логин:пароль\r\n\r\n" +
                                 $"{Profile.Login}:{Profile.Password}\r\n\r\n" +
                                 $"Torrserver доступен по адресу:\r\n" +
                                 $"{url}"
                                 ;
                    await DeleteMessage(idMessage);
                    await botClient.SendTextMessageAsync(AdminChat, result, replyMarkup: KeyboardManager.GetShowLogPassOther(result));
                    return;
                }

            }
            if (text.Contains("/edit_profile_"))
            {
                var uid = text.Split("/edit_profile_")[1]?.Replace("_","-");
                Console.WriteLine($"Пришел профиль на редактирование: [{uid}]");
                await DeleteMessage(idMessage);
                var profile = await SqlMethods.GetProfileUser(null, uid);
                if (profile is null)
                {
                    await botClient.SendTextMessageAsync(AdminChat, "Данный профиль не найден."
                        , replyMarkup: KeyboardManager.GetDeleteThisMessage());
                     return;
                }
                else
                {
                    await botClient.SendTextMessageAsync(AdminChat,profile.ToString(),replyMarkup: KeyboardManager.GetProfileEditOther(uid));
                     return ;
                }
            }
            #endregion OtherProfile


            if (text == "💾 Авто-бекап")
            {
                await DeleteMessage(idMessage);
                var messageBackup = ServerManagement.AutoBackupManager.GetBackupMessage();
                var setTorrBot = await SqlMethods.GetSettingsTorrserverBot();
                var isActive = setTorrBot.IsAutoBackupEnabled ? "🟢 Вкл" : "🔴 Выкл";
                var time = setTorrBot.AutoBackupTime;
                await botClient.SendTextMessageAsync(AdminChat,
                     "\u2699 *Настройка авто-бекапа* ☁\n" +
                     $"Состояние:{isActive}\n" +
                     $"Время авто-бекапа:⏰ {time} \n\n" +
                     $"🔄 При запуске авто-бекапа или его загрузке\n" +
                     $"Torrserver *временно отключается* для безопасного копирования файлов...\n"+
                     $"❗После редактирования настроек,\n " +
                     $"нужно перезагрузить сам сервер/бота ❗\n\n" +
                     $"{messageBackup}",
                     replyMarkup: KeyboardManager.GetMainBackups(setTorrBot.IsAutoBackupEnabled));
                return;

            }

            if (text == "⚙ Настройки")
            {
                await DeleteMessage(idMessage);
                await botClient.SendTextMessageAsync(AdminChat, "\u2699 Настройки", replyMarkup: KeyboardManager.GetSettingsMain());
                return;
            }
            if (text == "🔐 Доступ")
            {
                await DeleteMessage(idMessage);
                await botClient.SendTextMessageAsync(AdminChat,
                    "Управление профилями доступа к Torrserver."
                    , replyMarkup: KeyboardManager.GetProfilesUsersTorrserver());
                return;

            }
            if(text== "📊 Статус")
            {
                await DeleteMessage(idMessage);
                var result = await ServerInfo.GetStatusAsync();
                await botClient.SendTextMessageAsync(AdminChat,
                   $"📊 Общий статус системы\r\n\r\n{result}");

                return;
            }
                if (text == "🔐 Доступ2")
            {
                await DeleteMessage(idMessage);
                var setTorr = await SqlMethods.GetSettingsTorrserverBot();
                await SqlMethods.ListTablesAsync();
                await botClient.SendTextMessageAsync(AdminChat,
                    "Управление доступом к Torrserver.\r\n" + setTorr.ToString()
                    , replyMarkup: KeyboardManager.GetControlTorrserver());
                return;
            }
            if (text == "🔄 Перезагрузки")
            {
                await DeleteMessage(idMessage);
                await botClient.SendTextMessageAsync(AdminChat, "🔄 Перезагрузки\r\n" +
                    "Выберите что нужно перезагрузить !", replyMarkup: KeyboardManager.GetRestartingMain());
                return;
            }
            #endregion Обработка текстовых кнопок
            return;
        }

        private static async Task HandlerCallbackQueryCommands(CallbackQuery callbackQuery)
        {
            var callbackData = callbackQuery.Data;
            var idMessage = callbackQuery.Message.MessageId;

            try
            {
                #region Авто-бекап

                if (callbackData == "down-auto-backup")
                {
                    await botClient.DeleteMessageAsync(AdminChat,idMessage);
                    await TelegramBot.StartAutoBackup();
                    return;

                }
                if (callbackData.Contains("auto-backup"))
                {
                    var setTorrBot = await SqlMethods.GetSettingsTorrserverBot();
                    var messageBackup = ServerManagement.AutoBackupManager.GetBackupMessage();
                    if (callbackData.StartsWith("-1auto-backup"))
                    {
                        DateTime currentTime = DateTime.Parse(setTorrBot.AutoBackupTime);
                        setTorrBot.AutoBackupTime = currentTime.AddHours(-1).ToString("HH:mm");
                    }
                    else if (callbackData.StartsWith("+1auto-backup"))
                    {
                        DateTime currentTime = DateTime.Parse(setTorrBot.AutoBackupTime);
                        setTorrBot.AutoBackupTime = currentTime.AddHours(1).ToString("HH:mm");
                    }
                    else if (callbackData == "auto-backup0")
                    {
                        setTorrBot.IsAutoBackupEnabled = false;
                    }
                    else if (callbackData == "auto-backup1")
                    {
                        setTorrBot.IsAutoBackupEnabled = true;
                    }
                    await SqlMethods.UpdateSettingsTorrserverBot(setTorrBot);
                    var isActive = setTorrBot.IsAutoBackupEnabled ? "🟢 Вкл" : "🔴 Выкл";
                    var time = setTorrBot.AutoBackupTime;
                    await botClient.EditMessageTextAsync(AdminChat,idMessage,
                   "\u2699 *Настройка авто-бекапа* ☁\n" +
                   $"Состояние:{isActive}\n" +
                   $"Время авто-бекапа:⏰ {time} \n\n" +
                   "🔄 При запуске авто-бекапа или его загрузке\n" +
                   "Torrserver *временно отключается* для безопасного копирования файлов...\n" +
                   "❗После редактирования настроек,\n " +
                   "нужно перезагрузить сам сервер/бота ❗\n\n" +
                   $"{messageBackup}",
                   replyMarkup: KeyboardManager.GetMainBackups(setTorrBot.IsAutoBackupEnabled));

                }
                #endregion Авто-бекап
                #region Универсальные команды бота
                if (callbackData == "deletemessages")
                {
                    await DeleteMessage(idMessage);
                    await  SqlMethods.SwitchOffInputFlag();
                    return;
                }
                if (callbackData == "back_settings_main")
                {

                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "\u2699 Настройки", replyMarkup: KeyboardManager.GetSettingsMain());
                    return;
                }
                if (callbackData.Contains("exit"))
                {
                    var property = callbackData.Split("exit")[1];
                    await DeleteMessage(idMessage);
                    Console.WriteLine($"Выход из ввода {property}");
                    await SqlMethods.SwitchTorSettingsInputFlag(property, false);
                    var result = ParsingMethods.GetExitMessage(property);
                    await botClient.SendTextMessageAsync(AdminChat
                                                         , result
                                                         , replyMarkup: KeyboardManager.GetDeleteThisMessage());
                    return;
                }
                #endregion Универсальные команды бота
                #region Перезагрузки
                
                if (callbackData.Contains("auto_restart_torrserver"))
                {
                    var SettingsTorrserverBot =await SqlMethods.GetSettingsTorrserverBot();
    

                    
                    if (callbackData.StartsWith("+60"))
                    {
                        DateTime restartTime = DateTime.Parse(SettingsTorrserverBot.TorrserverRestartTime);
                        restartTime = restartTime.AddMinutes(60);
                        SettingsTorrserverBot.TorrserverRestartTime = restartTime.ToString("HH:mm");
                    }
                    else if (callbackData.StartsWith("-60"))
                    {
                        DateTime restartTime = DateTime.Parse(SettingsTorrserverBot.TorrserverRestartTime);
                        restartTime = restartTime.AddMinutes(-60);
                        SettingsTorrserverBot.TorrserverRestartTime = restartTime.ToString("HH:mm");
                    }
                    if (callbackData.EndsWith("0"))
                    {
                        SettingsTorrserverBot.IsTorrserverAutoRestart=false;
                    }
                    if (callbackData.EndsWith("1"))
                    {
                        SettingsTorrserverBot.IsTorrserverAutoRestart = true;
                    }
                    string result = $"🔧 Автоперезапуск Torrserver\n\n"
                                  + $"Состояние: {(SettingsTorrserverBot.IsTorrserverAutoRestart ? "🟢 ВКЛ" : "🔴 ВЫКЛ")}\n"
                                  + $"Время перезапуска: ⏰ {SettingsTorrserverBot.TorrserverRestartTime}\n\n"
                                  + "Рекомендуется выполнять перезагрузку раз в сутки\n"
                                  + "для стабильной работы профилей.\n" +
                                  "❗После редактирования настроек,нужно перезапустить сам сервер/бота.❗";

                    await SqlMethods.UpdateSettingsTorrserverBot(SettingsTorrserverBot);
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, result, replyMarkup: 
                          KeyboardManager.GetAutoRestartingTorrserverMain(SettingsTorrserverBot.IsTorrserverAutoRestart));
                    return;
                }
                if (callbackData == "restart_torrserver")
                {
                    await Torrserver.RebootingTorrserver();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "Torrserver перезагружен \u2705", replyMarkup: KeyboardManager.buttonHideButtots);
                    return;
                }
                if (callbackData == "restart_server")
                {

                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "Сервер(vps) будет перезагружен \u2705", replyMarkup: KeyboardManager.buttonHideButtots);
                    ServerControl.RebootServer();
                    return;
                }
                if(callbackData== "restart_thisbot")
                {
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "Бот будет перезагружен \u2705", replyMarkup: KeyboardManager.buttonHideButtots);
                    ServerControl.RestartBotService();
                    return;
                }
                #endregion Перезагрузки
                #region Работа с конфигом Torrserver (ARGS)
                if (callbackData.Contains("torrConfigSetOne"))
                {
                    await HandlerCallbackQuery.CheckSettingServerArgsAndExecute(callbackQuery, callbackData);
                    return;
                }
                if (callbackData == "setTorrArgsSetConfig")
                {
                    await Torrserver.RebootingTorrserver();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "Конфиг Torrserver обновлен! \u2705\r\n" +
                        "Torrserver перезагружен .", replyMarkup: KeyboardManager.GetShoWServerArgsConfig());
                    return;
                }
                if (callbackData == "resetTorrArgsSetConfig")
                {

                    await ServerArgsConfiguration.ResetConfig();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "Конфиг Torrserver сброшен по умолчанию ! \u2705", replyMarkup: KeyboardManager.GetShoWServerArgsConfig());
                    return;
                }
                if (callbackData == "showTorrArgssetInfo")
                {
                    var conf = await SqlMethods.GetArgsConfigTorrProfile(AdminChat);
                    var resultStringConfig = ServerArgsConfiguration.SerializeConfigArgs(conf);
                    var resultInfoArgsConfig = resultStringConfig + "\r\n\r\n" + conf.ToString();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, resultInfoArgsConfig, replyMarkup: KeyboardManager.GetShoWServerArgsConfig());
                    return;
                }
                if (callbackData.Contains("torrArgsSettings"))
                {
                    // torr_config
                    var startIndexKeySettings = Convert.ToInt32(callbackData.Split("torrArgsSettings")[0]);
                    await SqlMethods.SwitchOffInputFlag();
                    var config = await SqlMethods.GetArgsConfigTorrProfile(AdminChat);
                    Console.WriteLine("Настройки Torrserver(args) ");
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "🛠️ Конфиг Torrserver ."
                        , replyMarkup: KeyboardManager.GetServerArgsConfigMain(AdminChat, config, startIndexKeySettings)
                        );
                    return;
                }
                #endregion Работа с конфигом Torrserver (ARGS)
                #region Работа с настройками Torrserver
                if (callbackData.Contains("torrSettings"))
                {
                    var startIndexKeySettings = Convert.ToInt32(callbackData.Split("torrSettings")[0]);
                    await SqlMethods.SwitchOffInputFlag();
                    var config = await SqlMethods.GetSettingsTorrProfile(AdminChat);
                    Console.WriteLine("Настройки Torrserver");
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "⚙️ Настройки Torrserver ."
                        , replyMarkup: await KeyboardManager.GetBitTorrConfigMain(AdminChat, config, startIndexKeySettings)
                        );
                    return;
                }
                if (callbackData.Contains("torrSetOne"))
                {

                    await HandlerCallbackQuery.CheckSettingAndExecute(callbackQuery, callbackData);

                    return;
                }
                if (callbackData == "setTorrSetConfig")
                {
                    await Torrserver.RebootingTorrserver();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "Настройки Torrserver обновленны! \u2705\r\n" +
                        "Torrserver перезагружен .", replyMarkup: KeyboardManager.GetShoWBitTorrConfig());
                    return;
                }
                if (callbackData == "resetTorrSetConfig")
                {
                    await BitTorrConfigation.ResetConfig();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "Настройки Torrserver сброшены по умолчанию ! \u2705", replyMarkup: KeyboardManager.GetShoWBitTorrConfig());
                    return;
                }
                if (callbackData == "showTorrsetInfo")
                {
                    var config = await SqlMethods.GetSettingsTorrProfile(AdminChat);
                    var resultInfoTorrSettings = config.ToString();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, resultInfoTorrSettings, replyMarkup: KeyboardManager.GetShoWBitTorrConfig());
                    return;
                }
                #endregion Работа с настройками Torrserver
                #region Настройки сервера
                if (callbackData.Contains("set_server_bbr"))
                {
                    bool? enable = callbackData.StartsWith("1") ? true
                                 : callbackData.StartsWith("0") ? false
                                 : null;

                    if (enable.HasValue)
                    {
                        await ServerControl.SetBbrState(enable.Value);
                    }

                    var isBBRActive = ServerInfo.CheckBBRConfig();
                    string bbrStatus = isBBRActive ? "\u2705" : "\u274C";

                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "Состояние BBR на сервере: " + bbrStatus,
                        replyMarkup: KeyboardManager.GetSetServerBbrMain(isBBRActive));
                    return;
                }

                if (callbackData == "set_server")
                {
                    Console.WriteLine("Настройки сервера");
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "💻 Настройки сервера"
                        , replyMarkup: KeyboardManager.GetSetServerMain());
                    return;
                }
                #endregion Настройки сервера
                #region Настройки бота
                if (callbackData == "set_bot")
                {
                    Console.WriteLine("Настройки бота");
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "🤖 Настройки бота"
                        , replyMarkup: KeyboardManager.GetSettingsBot()
                        );
                    return;
                }

                if (callbackData.Contains("time_zone"))
                {
                    var timezoneChangeIndicator = callbackData.Split("time_zone")[0];
                    Console.WriteLine($"Пришел timezoneChangeIndicator: {timezoneChangeIndicator}");
                    if (timezoneChangeIndicator == "+" || timezoneChangeIndicator == "-")
                    {
                        await SqlMethods.SwitchTimeZone(timezoneChangeIndicator);
                    }
                    var settingBot = await SqlMethods.GetSettingBot();
                    var timeLocalServer = ServerInfo.GetLocalServerTime();
                    var localTimeZone = ServerInfo.GetLocalServerTimeTimeZone();
                    string localTimeZoneString = $"UTC{(localTimeZone >= 0 ? "+" : "")}{localTimeZone}";
                    await botClient.EditMessageTextAsync(AdminChat, idMessage,

                        $"\uD83D\uDD52 Время сервера : {timeLocalServer}  🌍 {localTimeZoneString}\r\n" +
                        $"🌍 Ваш часовой пояс: {settingBot.TimeZoneOffset} UTC",
                        replyMarkup: KeyboardManager.GetMainTimeZone());
                    return;

                }
                #endregion Настройки бота
                #region Управление пользователями(torrserver)

                
                if(callbackData== "exitLoginPasswordOtherProfile")
                {
                    await SqlMethods.SwitchOffInputFlag();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage,
                "Вы вышли из режима ввода логина/пароля "
                , replyMarkup: KeyboardManager.buttonHideButtots);
                    return;
                }
                if (callbackData.Contains("delOther"))
                {
                    var uid = callbackData.Split("delOther")[1];
                    await SqlMethods.DeleteProfileOther(uid);
                    await botClient.EditMessageTextAsync(AdminChat, idMessage,
                     "Профиль удален \u2705\r\n" +
                     "Изменения вступят в силу после перезагрузки Torrserver через меню в боте"
                     , replyMarkup: KeyboardManager.buttonHideButtots);
                    return;
                }
                if (callbackData.Contains("mainDeleOth"))
                {
                    var uid = callbackData.Split("mainDeleOth")[1];
                    var p = await SqlMethods.GetProfileUser(null, uid);
                    await botClient.EditMessageTextAsync(AdminChat, idMessage,         
                     "Удаление профиля \u2199\r\n" +
                      $"\uD83D\uDC64 Логин:{p.Login} \r\n" +
                     $"/edit_profile_{uid.Replace("-","_")}\r\n" +
                     $"Подтвердите удаление \u2757"
                     , replyMarkup: KeyboardManager.DeletePfofileOther(uid));
                    return;
                }
                if (callbackData.Contains("mainNoteOth"))
                {
                    var uid = callbackData.Split("mainNoteOth")[1];
                    var p = await SqlMethods.GetProfileUser(null,uid);
                    var note = p?.AdminComment ?? "заметка отсутствует";
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagNoteOtherProfile", true);
                    await SqlMethods.SetLastChangeUid(uid);
                    await botClient.EditMessageTextAsync(AdminChat, idMessage,
                     "Заметка профиля ↙\r\n" +
                      $"\uD83D\uDC64 Логин:{p.Login} \r\n" +
                    $"/edit_profile_{uid.Replace("-", "_")}\r\n"+
                    $"\uD83D\uDCDD Заметка: {note}\r\n" +
                    $"\u270D Вы в режиме ввода заметки \u2757\r\n" +
                    $"Ограничение:300 символов."
                     , replyMarkup: KeyboardManager.ExitEditNoteOtherPfofile());
                    return;
                }
                if (callbackData.Contains("setAccOther"))
                {
                        var parts = callbackData.Split("setAccOther");
                        var uid = parts[1];
                        var p = await SqlMethods.GetProfileUser(null, uid);
                    if (!string.IsNullOrEmpty(parts[0]) && !string.IsNullOrEmpty(parts[1]))
                    {
                        var dayAccess = Convert.ToInt32(parts[0]);
                        if (dayAccess == 0)
                        {
                            p.AccessEndDate = DateTime.UtcNow;
                        }
                        else
                        {
                            if (p.AccessEndDate.HasValue)
                            {
                                p.AccessEndDate = p.AccessEndDate.Value.AddDays(dayAccess);
                            }
                            else
                            {
                                p.AccessEndDate = DateTime.UtcNow.AddDays(dayAccess);
                            }
                        }
                        await SqlMethods.EddingProfileUser(p);
                    }
                    var builder = new StringBuilder();
                        var remainingTime = p.AccessEndDate.HasValue
                                          ? p.AccessEndDate.Value - DateTime.UtcNow
                                          : (TimeSpan?)null;
                        builder.AppendLine($"Управление доступом профиля ↙");
                    builder.AppendLine($"👤 {(p.AccessEndDate == null || p.AccessEndDate > DateTime.Now ? "🟢" : "🔴")} Логин: {p.Login}");

                    builder.AppendLine($"⏳ Окончание доступа: {(p.AccessEndDate.HasValue ? p.AccessEndDate.Value.ToString("dd.MM.yyyy HH:mm") : "Не задано")}");
                        if (remainingTime.HasValue && remainingTime.Value.TotalMilliseconds > 0)
                        {
                            builder.AppendLine($"🕒 Осталось: {remainingTime.Value.Days} дн. {remainingTime.Value.Hours} ч.");
                        }
                        else
                        {
                            builder.AppendLine($"🕒 Осталось: Не задано или доступ истёк");
                        }
                        builder.AppendLine($"/edit_profile_{uid.Replace("-", "_")}");
                    builder.AppendLine($"После изменений,требуется перезагрузка Torrserver.\r\nВ разделе бота \"Перезагрузки\"");
                        await botClient.EditMessageTextAsync(AdminChat, idMessage,
                          builder.ToString()
                       , replyMarkup: KeyboardManager.GetAccessControlOther(uid));
                    
                    return;
                }
                if (callbackData.Contains("mainLogPassOth"))
                {
                    var uid = callbackData.Split("mainLogPassOth")[1];
                    var p = await SqlMethods.GetProfileUser(null, uid);
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagLoginPasswordOtherProfile",true);
                    await SqlMethods.SetLastChangeUid(uid);
                    await botClient.EditMessageTextAsync(AdminChat, idMessage,
                     "Смена логина/пароля профиля ↙\r\n" +
                     $"\uD83D\uDC64 Логин:{p.Login} \r\n" +
                     $"/edit_profile_{uid.Replace("-", "_")} \r\n" + 
                     $"✍ Вы в режиме ввода логина и пароля \u2757\r\n" + 
                     $"Ограничения \u2199\r\n" + 
                     $"\u2757 Логин и пароль может содержать только английские буквы и цифры.\r\n" + 
                     $"\u2757 Логин и пароль может состоять только из 20 символов.\r\n" +
                     $" Пример \u2199\r\n" +
                     $"ivanpetrov:j4jjkj4o4i433\r\n" +
                     $"\u2139 слева логин(max 20 симв.) : справа пароль(max 20 симв.)" 
                     , replyMarkup: KeyboardManager.ExitEditLoginPasswordOtherProfile());
                    return;
                }
                if (callbackData == "BackProfilesUersTorrserver")
                {
                    await botClient.EditMessageTextAsync(AdminChat, idMessage,
                    "Управление профилями доступа к Torrserver."
                    , replyMarkup: KeyboardManager.GetProfilesUsersTorrserver());
                    return;
                }
                if(callbackData == "createAutoNewProfileOther")
                {
                    await SqlMethods.SwitchOffInputFlag();
                    var newProfile = await SqlMethods.CreateAuthoNewProfileOther();
                    if(newProfile != null) 
                    {
                    var uidNewProfile = newProfile.UniqueId.ToString();
                    var login = newProfile.Login;
                    var password = newProfile.Password;
                    await botClient.EditMessageTextAsync(AdminChat, idMessage,
                     $"Новый пользователь сгенерирован ✅\r\n" +
                     $"🔐 Доступ дан на 24 часа по умолчанию.\r\n" + 
                     $"Будет активен после перезагрузки Torrserver через меню в боте\r\n" + 
                     $"/showlogpass_{login}_{password}\r\n" + 
                     $"/edit_profile_{uidNewProfile.Replace("-", "_")}"
                    , replyMarkup: KeyboardManager.buttonHideButtots);
                        return;
                     }
                    else
                    {
                        await botClient.EditMessageTextAsync(AdminChat, idMessage,
                          $"Не удалось сгенерировать пользователя."
                         , replyMarkup: KeyboardManager.buttonHideButtots);
                    }
                    return;
                }
                if(callbackData == "createNewProfile")
                {
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagNewLoginAndPasswordOtherProfile", true);
                    await botClient.EditMessageTextAsync(AdminChat, idMessage,
                      $"Само создание еще не готово.Пока можно сгенерировать или выйти из режима ввода.\r\n" +
                      $"✍ Вы в режиме создания нового пользователя\u2757\r\n" +
                      $"Введите новый логин и пароль \r\n" +
                      $"Ограничения \u2199\r\n" +
                      $"\u2757 Логин и пароль может содержать только английские буквы и цифры.\r\n" +
                      $"\u2757 Логин и пароль может состоять только из 20 символов.\r\n" +
                      $" Пример \u2199\r\n" +
                      $"ivanpetrov:j4jjkj4o4i433\r\n" +
                      $"\u2139 слева логин(max 20 симв.) : справа пароль(max 20 симв.)"
                      , replyMarkup: KeyboardManager.CreateNewProfileTorrserverUser());
                    return;
                }
                if(callbackData == "clearOtherProfiles")
                {
                    await botClient.EditMessageTextAsync(AdminChat, idMessage,
                        $"Вы собираетесь удалить все дополнительные профили.\r\n" +
                        $"⚠️ Это действие необратимо!\r\n" +
                        $"Подтвердите удаление или отмените.",
                        replyMarkup: KeyboardManager.ConfirmDeleteAllOtherProfiles());
                    return;
                }

                if (callbackData == "confirmDeleteAllOtherProfiles")
                {
                    var profilesExist = await SqlMethods.WithDbContextAsync(async db =>
                        await db.Profiles.AnyAsync());

                    if (!profilesExist)
                    {
                        await botClient.EditMessageTextAsync(AdminChat, idMessage,
                            $"ℹ️ Дополнительных профилей для удаления не найдено.",
                            replyMarkup: KeyboardManager.buttonHideButtots);
                        return;
                    }

                    bool result = await SqlMethods.DeleteAllOtherProfiles();

                    if (result)
                    {
                        await botClient.EditMessageTextAsync(AdminChat, idMessage,
                            $"✅ Все дополнительные профили были удалены.\r\n" +
                            $"Изменения вступят в силу после перезагрузки Torrserver через меню в боте.",
                            replyMarkup: KeyboardManager.buttonHideButtots);
                    }
                    else
                    {
                        await botClient.EditMessageTextAsync(AdminChat, idMessage,
                            $"⚠️ При удалении возникла ошибка.\r\n" +
                            $"Подробности можно посмотреть в логах.",
                            replyMarkup: KeyboardManager.buttonHideButtots);
                    }

                    return;
                }


                if (callbackData.Contains("OtherProfiles"))
                {
                    try
                    {
                        await SqlMethods.SwitchOffInputFlag();
                        var (skipCount, sort) = ParsingMethods.ParseOtherProfilesCallback(callbackData);
                        var result = "Список профилей: ошибка обновления профилей";
                        var updateProfiles = await Torrserver.UpdateAllProfilesFromConfig();
                        var countAllProfiles = 0;
                        var countActive= 0;
                        var nextCount = 0;
                        if (updateProfiles)
                        {
                            countAllProfiles = await SqlMethods.GetCountAllProfiles();
                            countActive = await SqlMethods.GetActiveProfilesCount();
                            var profiles = await SqlMethods.GetAllProfilesUser(skipCount, sort);

                            if (profiles != null && profiles.Count > 0)
                            {
                                nextCount = skipCount + profiles.Count;
                                result = ParsingMethods.FormatProfilesList(profiles,countActive, countAllProfiles,skipCount ,sort);
                            }
                            else
                            {
                                result = "Список профилей пуст";
                            }
                        }
                        var keyboard = KeyboardManager.GetControlOtherProfilesTorrserver(nextCount, countAllProfiles, sort);
                        await botClient.EditMessageTextAsync(
                            AdminChat,
                            idMessage,
                            result,
                            replyMarkup: keyboard
                            ,parseMode: ParseMode.MarkdownV2
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return;
                    }
                    return;
                }


                if (callbackData== "MainProfile")
                {
                    var setTorr = await SqlMethods.GetSettingsTorrserverBot();
                    await SqlMethods.ListTablesAsync();
                    await botClient.EditMessageTextAsync(AdminChat,idMessage,
                        "Управление главным профилем  Torrserver.\r\n" + setTorr.ToString()+"" +
                        "\r\nПри редактировании данных профиля,\r\n" +
                        "Torrserver перезагружается\n" +
                        "❗При редактировании автосмены пароля,после нужно перезапустить бота .❗"
                        , replyMarkup: KeyboardManager.GetControlTorrserver());
                    return;
                }


                if (callbackData == "manage_login_password")
                {
                    Console.WriteLine("Управление логином и паролем Torrserver");
                    await SqlMethods.SwitchOffInputFlag();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "Управление логином и паролем Torrserver ."

                     , replyMarkup: KeyboardManager.GetNewLoginPasswordMain());
                    return;
                }

                if (callbackData == "сontrolTorrserver")
                {
                    Console.WriteLine("Управление доступом к Torrserver .");
                    var setTorr = await SqlMethods.GetSettingsTorrserverBot();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage, "Управление доступом к Torrserver.\r\n" + setTorr.ToString()

                        , replyMarkup: KeyboardManager.GetControlTorrserver());
                    return;
                }
                if (callbackData == "set_login_manually")
                {
                    Console.WriteLine("В режите ввода логина.");
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagLogin",true);
                    await botClient.EditMessageTextAsync(AdminChat, idMessage
                                                        , "\u2757 Вы в режиме ввода логина .\r\n" +
                                                        "Напишите желаемый логин.\r\n" +
                                                        "\u2757 Login может содержать только английские буквы и цифры.\r\n" +
                                                         "Ограничение на символы:10"
                                                        , replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("Login", 0 ));
                    return;
                }
                if (callbackData == "set_password_manually")
                {
                    Console.WriteLine("В режите ввода пароля.");
                    await SqlMethods.SwitchTorSettingsInputFlag("FlagPassword",true);
                    await botClient.EditMessageTextAsync(AdminChat, idMessage
                                                        , "\u2757 Вы в режиме ввода пароля .\r\n" +
                                                        "Напишите желаемый пароль.\r\n" +
                                                        "\u2757 Password может содержать только английские буквы и цифры.\r\n" +
                                                         "Ограничение на символы:10"
                                                        , replyMarkup: KeyboardManager.CreateExitBitTorrConfigInputButton("Password",0));
                    return;

                }
                if (callbackData == "generate_new_login")
                {
                    await Torrserver.ChangeMainAccountTorrserver("", "", true, false);
                    await botClient.EditMessageTextAsync(AdminChat, idMessage
                                                         , "Логин успешно изменен !"
                                                         , replyMarkup: KeyboardManager.GetNewLoginPasswordMain());
                    Console.WriteLine("Логин успешно изменен !");
                    return;
                }
                if (callbackData == "generate_new_password")
                {

                    await Torrserver.ChangeMainAccountTorrserver("", "", false, true);
                    await botClient.EditMessageTextAsync(AdminChat, idMessage
                                                         , "Пароль успешно изменен !"
                                                         , replyMarkup: KeyboardManager.GetNewLoginPasswordMain());
                    Console.WriteLine("Пароль успешно изменен !");
                    return;
                }
                if (callbackData == "show_login_password")
                {

                    var passw = Torrserver.TakeMainAccountTorrserver();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage
                                                             , $"{passw}"
                                                             , replyMarkup: KeyboardManager.GetNewLoginPasswordMain());
                    Console.WriteLine($"Запрос на просмотр логина:({passw}).");
                    return;
                }

                if (callbackData == "change_time_auto" || callbackData.Contains("setAutoPassMinutes"))
                {

                    if (callbackData.Contains("setAutoPassMinutes"))
                    {
                        var setMinutesAutoChangePassTorr = ParsingMethods.ExtractTimeChangeValue(callbackData);
                        await SqlMethods.SetTimeAutoChangePasswordTorrserver(setMinutesAutoChangePassTorr);
                    }


                    var setTorr = await SqlMethods.GetSettingsTorrserverBot();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage
                                                       , $"Установка времени автосмены пароля .\r\n" +
                                                         $"Сейчас стоит {setTorr.TimeAutoChangePassword}"
                                                       , replyMarkup: KeyboardManager.GetSetTimeAutoChangePassword());
                    return;
                }
                if (callbackData == "print_time_auto")
                {

                    var setTorr = await SqlMethods.GetSettingsTorrserverBot();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage
                                                       , $"⏰ Время автосмены пароля {setTorr.TimeAutoChangePassword}"
                                                       , replyMarkup: KeyboardManager.GetControlTorrserver());
                    return;
                }
                if (callbackData == "enable_auto_change")
                {
                    var setTorr = await SqlMethods.GetSettingsTorrserverBot();
                    await SqlMethods.SwitchAutoChangePassTorrserver(true);
                    await botClient.EditMessageTextAsync(AdminChat, idMessage
                                                       , "Автосмена пароля включена \u2705 \r\n" +
                                                      setTorr.TimeAutoChangePassword
                                                       , replyMarkup: KeyboardManager.GetControlTorrserver());
                    return;
                }
                if (callbackData == "disable_auto_change")
                {

                    var setTorr = await SqlMethods.GetSettingsTorrserverBot();
                    await SqlMethods.SwitchAutoChangePassTorrserver(false);
                    await botClient.EditMessageTextAsync(AdminChat, idMessage
                                                       , "Автосмена пароля выключена \u2705 \r\n" +
                                                      setTorr.TimeAutoChangePassword
                                                       , replyMarkup: KeyboardManager.GetControlTorrserver());
                    return;
                }
                if (callbackData == "show_status")

                {

                    var settings = await SqlMethods.GetSettingsTorrserverBot();
                    await botClient.EditMessageTextAsync(AdminChat, idMessage
                                                       , settings.ToString()
                                                       , replyMarkup: KeyboardManager.GetControlTorrserver());
                    return;

                }
                #endregion  Управление пользователями(torrserver)
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            }


            return;
        }

        public static bool IsTextCommandBot(string command)
        {
            if (string.IsNullOrEmpty(command)) return false;

            HashSet<string> commands = new HashSet<string>()
                {
                    "/start",
                    "🔐 Доступ",
                    "💾 Авто-бекап",
                    "⚙ Настройки",
                    "🔄 Перезагрузки",
                    "📊 Статус"
                };

            if (commands.Contains(command))
                return true;
            if (command.StartsWith("/edit_profile_")) return true;
            if (command.StartsWith("/showlogpass_")) return true;

            return false;
        }

        public static bool IsCallbackQueryCommandBot(string command)
        {
            HashSet<string> commands = new HashSet<string>()
            {
            "deletemessages"
            ,"manage_login_password"
            ,"generate_new_password"
            ,"set_password_manually"
            ,"generate_new_login"
            ,"set_login_manually"
            ,"show_login_password"
            ,"change_time_auto"
            ,"print_time_auto"
            , "enable_auto_change"
            ,"disable_auto_change"
            ,"show_status"
            ,"сontrolTorrserver"
            ,"setAutoPassMinutes"
            ,"-time_zone"
            ,"+time_zone"
            ,"time_zone"
            ,"torrArgsSettings"
            ,"set_server"
            ,"set_bot"

            ,"back_settings_main"
            ,"set_server_bbr"

            ,"restart_torrserver"
            ,"restart_thisbot"
            ,"restart_thisserver"
            ,"showTorrsetInfo"
            ,"resetTorrSetConfig"
            ,"setTorrSetConfig"
            ,"showTorrArgssetInfo"
            ,"resetTorrArgsSetConfig"
            ,"setTorrArgsSetConfig"
            ,"OtherProfiles"
            ,"MainProfile"
            ,"BackProfilesUersTorrserver"
            ,"createNewProfile",
            "clearOtherProfiles",
            "confirmDeleteAllOtherProfiles"
            ,"createAutoNewProfileOther"
            ,"auto_restart_torr"
            ,"down-auto-backup"
            };
            if (commands.Contains(command))
            {
                return true;
            }
            if (commands.Contains("exit"))
            {
                return true;
            }
            if (commands.Contains("TorrSett"))
            {
                return true ;
            }
            if (commands.Contains("torrArgsSettings"))
            {
                return true;
            }
            if (command.Contains("setAutoPassMinutes"))
            {
                string valuePart = command.Split("setAutoPassMinutes")[0].Trim();
                if (int.TryParse(valuePart, out _))
                {
                    return true;
                }
            }
            if (command.Contains("torrSettings"))
            {
                return true;
            }
            if (command.Contains("torrConfigSetOne"))
            {
                return true;
            }
            if (command.Contains("torrSetOne"))
            {
                return true;
            }
            if (command.Contains("torrConf"))
            {
                return true;
            }
            if (command.Contains("OtherProfiles"))
            {
                return true;
            }
            if (command.Contains("/edit_profile_"))
            {
                return true;
            }
            if (command.Contains("/showlogpass_"))
            {
                return true;
            }
            if (command.Contains("mainLogPassOth"))
            {
                return true;
            }
            if (command.Contains("mainAccessOth"))
            {
                return true;
            }
            if (command.Contains("mainNoteOth"))
            {
                return true;
            }
            if (command.Contains("mainDeleOth"))
            {
                return true;
            }
            if (command.Contains("delOther"))
            {
                return true;
            }
            if (command.Contains("set_auto_restart_time"))
            {
                return true;
            }
            if (command.Contains("auto_restart_torrserver"))
            {
                return true;
            }
            if (command.Contains("auto-backup"))
            {
                return true;
            }
            {
                string valuePart = command.Split(command)[0].Trim();
                if (int.TryParse(valuePart, out _))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
