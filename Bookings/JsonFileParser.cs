using System.Text.Json;

namespace Bookings;

// TODO: Add interface and DI?
internal static class JsonFileParser
{
    public static T Parse<T>(string file)
    {
        if (Path.GetExtension(file) != ".json")
        {
            throw new BookingsException($"File \"{file}\" has invalid extension. Only .json files are supported.");
        }

        // TODO: Add tests for null?
        return JsonSerializer.Deserialize<T>(File.ReadAllText(file));
    }
}
