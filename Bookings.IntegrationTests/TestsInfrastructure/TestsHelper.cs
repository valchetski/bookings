using System.Text.Json;

namespace Bookings.IntegrationTests.TestsInfrastructure;

internal static class TestsHelper
{
    public const string BookingsFilePath = "./TestsInfrastructure/Data/bookings.json";
    public const string HotelsFilePath = "./TestsInfrastructure/Data/hotels.json";

    public static string CreateTempFile<T>(T data)
    {
        if (!Directory.Exists("TempData"))
        {
            Directory.CreateDirectory("TempData");
        }

        var tempFile = $"TempData/{Guid.NewGuid()}.json";
        File.WriteAllText(tempFile, JsonSerializer.Serialize(data));
        return tempFile;
    }

    public static string CaptureConsoleOutput(Action action)
    {
        var output = new StringWriter();
        Console.SetOut(output);
        action();
        return output.ToString();
    }
}
