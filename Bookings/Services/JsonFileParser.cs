using System.Text.Json;

namespace Bookings.Services;

internal class JsonFileParser : IFileParser
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public JsonFileParser()
    {
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        _jsonSerializerOptions.Converters.Add(new DateFormatConverter());
    }

    public T? Parse<T>(string file)
    {
        if (Path.GetExtension(file) != ".json")
        {
            throw new BookingsException($"File \"{file}\" has invalid extension. Only .json files are supported.");
        }

        return JsonSerializer.Deserialize<T>(File.ReadAllText(file), _jsonSerializerOptions);
    }
}
