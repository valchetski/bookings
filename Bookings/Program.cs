using System.Text.RegularExpressions;
using Bookings.Models;
using Bookings.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings;

public static partial class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Usage: Bookings.exe --hotels hotels.json --bookings bookings.json");
            return;
        }

        string? hotelsFile = null;
        string? bookingsFile = null;

        for (int i = 0; i < args.Length; i += 2)
        {
            if (args[i] == "--hotels")
            {
                hotelsFile = args[i + 1];
            }
            else if (args[i] == "--bookings")
            {
                bookingsFile = args[i + 1];
            }
        }

        if (hotelsFile == null || bookingsFile == null)
        {
            Console.WriteLine("Usage: Bookings.exe --hotels hotels.json --bookings bookings.json");
            return;
        }

        var services = new ServiceCollection()
            .AddSingleton<ICommandHandler, CommandHandler>()
            .AddSingleton<IFileParser, JsonFileParser>()
            .AddSingleton<IAvailabilityService, AvailabilityService>()
            .BuildServiceProvider();

        try
        {
            var jsonFileParser = services.GetRequiredService<IFileParser>();

            var hotels = jsonFileParser.Parse<List<Hotel>>(hotelsFile);
            var bookings = jsonFileParser.Parse<List<Booking>>(bookingsFile);
            if (hotels == null || bookings == null)
            {
                Console.WriteLine("No data in files.");
                return;
            }

            ShowHelp();

            var commandRegexp = CommandRegex();
            var commandHandler = services.GetRequiredService<ICommandHandler>();

            while (true)
            {
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }

                var match = commandRegexp.Match(input);
                if (!match.Success)
                {
                    Console.WriteLine("Input is invalid.");
                    ShowHelp();
                    continue;
                }

                var parameters = match.Groups[2].Value
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToArray();
                switch (match.Groups[1].Value)
                {
                    case "Availability":
                        Console.WriteLine(commandHandler.HandleAvailabilityCommand(hotels, bookings, parameters));
                        break;
                    case "Search":
                        Console.WriteLine(commandHandler.HandleSearchCommand(hotels, bookings, parameters));
                        break;
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (BookingsException ex) 
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unknown error: " + ex.Message);
        }
    }

    [GeneratedRegex(@"^(Availability|Search)\(([^)]+)\)$")]
    private static partial Regex CommandRegex();

    private static void ShowHelp()
    {
        Console.WriteLine("To check room availability, use the command like: Availability(H1, 20240101-20240102, SGL)");
        Console.WriteLine("To search for available rooms, use command like: Search(H1, 365, SGL)");
        Console.WriteLine("To exit, press Enter.");
    }
}