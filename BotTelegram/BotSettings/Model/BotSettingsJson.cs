
   namespace AdTorrBot.BotTelegram.BotSettings.Model
    {
    public class BotSettingsJson
    {
        public string? YourBotTelegramToken { get; set; }
        public string? AdminChatId { get; set; }
        public string? FilePathTorrserver { get; set; }
        public bool Validate(out List<string> missingFields)
        {
            missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(YourBotTelegramToken))
                missingFields.Add("YourBotTelegramToken");

            if (string.IsNullOrWhiteSpace(AdminChatId))
                missingFields.Add("AdminChatId");

            if (string.IsNullOrWhiteSpace(FilePathTorrserver))
                missingFields.Add("FilePathTorrserver");

            return missingFields.Count == 0;
        }

    }

}
