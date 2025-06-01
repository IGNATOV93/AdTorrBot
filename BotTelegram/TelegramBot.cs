using AdTorrBot.BotTelegram.Db;
using AdTorrBot.BotTelegram.Handler;
using AdTorrBot.ServerManagement;
using FluentScheduler;
using AdTorrBot.BotTelegram.BotSettings;
using AdTorrBot.BotTelegram.BotSettings.Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdTorrBotTorrserverBot.BotTelegram
{
    public class TelegramBot
    {
        static public BotSettingsJson settingsJson =  BotSettingsMethods.LoadSettings();

        static readonly public TelegramBotClient client = new TelegramBotClient(settingsJson.YourBotTelegramToken);
        public  static string AdminChat = settingsJson.AdminChatId;  

        public static  InlineKeyboardMarkup inlineKeyboarDeleteMessageOnluOnebutton = new InlineKeyboardMarkup(new[]
                {new[]{InlineKeyboardButton.WithCallbackData("Скрыть \U0001F5D1", "deletemessages")}});
        public static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            try
            {
                var Message = update.Message;
                var ChatId = update?.CallbackQuery?.Message?.Chat?.Id.ToString();
                var InputText = Message?.Text;
                var InlineText = update?.CallbackQuery?.Data;
                if (update?.CallbackQuery?.Data != null)
                {
                    if (ChatId != AdminChat && !MessageHandler.IsCallbackQueryCommandBot(InlineText)) { return; }
                    await MessageHandler.HandleUpdate(update);
                    return;

                }
                if (Message?.Text != null)
                {
                    ChatId = Message.Chat.Id.ToString();
                    var textInputFlags = await SqlMethods.GetTextInputFlag();
                    if (ChatId==AdminChat&&textInputFlags.CheckAllBooleanFlags()) 
                     {  
                      await MessageHandler.HandleUpdate(update)
                      ;return;
                     }
                    if (ChatId != AdminChat && !MessageHandler.IsTextCommandBot(InputText)) 
                     { return; }

                    await MessageHandler.HandleUpdate(update);
                    return;

                }
            }
            catch (Exception ex) 
            {
             Console.WriteLine(ex.ToString());  
            }
            return;
        }
      
        public static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            Console.WriteLine(arg2.Message);
            return Task.CompletedTask;
        }
        
        static public async Task StartBot()
        {
           
            await SqlMethods.CheckAndInsertDefaultData();
            JobManager.Initialize(new MyRegistry());
            var statrBotTask = Task.Run(() => client.StartReceiving(Update, Error));
            await Task.Delay(1000);
            await Torrserver.Torrserver.UpdateAllProfilesFromConfig();
            await SendMessageToAdmin("Бот успешно стартовал!");
            Console.ReadLine();
        }

        public static async Task StartAutoBackup()
        {
            var messageSend = await client.SendTextMessageAsync(AdminChat, "⚠️ Torrserver временно отключен.\n Начинается резервное копирование файлов...");
            var isDoneBackup = await Task.Run(() => AdTorrBot.ServerManagement.AutoBackupManager.CreateBackupArchive());
            if (isDoneBackup)
            {
                string archivePath = AutoBackupManager.GetBackupArchivePath();
                await using FileStream stream = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
                await client.SendDocumentAsync(AdminChat, new InputFileStream(stream, Path.GetFileName(archivePath)));
                await client.DeleteMessageAsync(AdminChat, messageSend.MessageId);
            }
            else
            {
                await client.SendTextMessageAsync(AdminChat, "❌ Ошибка при создании архива, попробуйте позже.");
            }

            return;
        }
        public static async Task SendMessageToAdmin(string mes)
        {
           // Console.WriteLine(mes);
            try
            {
                await client.SendTextMessageAsync(AdminChat, mes, replyMarkup: inlineKeyboarDeleteMessageOnluOnebutton);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке сообщения: {ex.Message}");
            }
            return;
        }
    }
}
