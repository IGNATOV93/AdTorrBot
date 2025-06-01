using System.Text.Json;
using System.Text.Json.Serialization;
namespace AdTorrBot.BotTelegram
{
    public class CacheSizeConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetInt64() / (1024 * 1024);
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value * 1024 * 1024);
        }
    }
}
