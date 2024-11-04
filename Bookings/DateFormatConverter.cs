using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bookings;

public class DateFormatConverter()
    : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        return DateUtils.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DateUtils.Format(value));
    }
}