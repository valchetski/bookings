using System.Text.Json;

namespace Bookings.IntegrationTests.TestsInfrastructure;

internal static class TestsHelper
{
    public const string BookingsFilePath = "./TestsInfrastructure/Data/bookings.json";
    public const string HotelsFilePath = "./TestsInfrastructure/Data/hotels.json";
    private static readonly object _sync = new();

    public static string CreateTempFile<T>(T data)
    {
        if (!Directory.Exists("TempData"))
        {
            Directory.CreateDirectory("TempData");
        }

        var tempFile = $"TempData/{Guid.NewGuid()}.json";
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new DateFormatConverter());
        File.WriteAllText(tempFile, JsonSerializer.Serialize(data, options));
        return tempFile;
    }

    public static string CaptureConsoleOutput(Action action)
    {
        // it'll run tests in parallel, but console instance is one
        // simplest approach is just apply lock
        lock (_sync)
        {
            var output = new StringWriter();
            Console.SetOut(output);
            action();
            return output.ToString();
        }
    }
}
