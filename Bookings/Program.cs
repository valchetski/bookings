using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Bookings.Models;

namespace Bookings;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Usage: myapp --hotels hotels.json --bookings bookings.json");
            return;
        }

        string hotelsFile = null;
        string bookingsFile = null;

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
            Console.WriteLine("Usage: myapp --hotels hotels.json --bookings bookings.json");
            return;
        }

        try
        {
            // TODO: Maybe safe hotels and bookings in some service?
            List<Hotel> hotels = JsonFileParser.Parse<List<Hotel>>(hotelsFile);
            List<Booking> bookings = JsonFileParser.Parse<List<Booking>>(bookingsFile);
            var commandRegexp = new Regex(@"^(Availability|Search)\(([^)]+)\)$");

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
                    Console.WriteLine("To check room availability, use the format: Availability(hotelId, dateRange, roomType)");
                    Console.WriteLine("To search for available rooms, use the format: Search(hotelId, daysAhead, roomType)");
                    Console.WriteLine("To exit, press Enter.");
                    continue;
                }

                var parameters = match.Groups[2].Value
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToArray();
                switch (match.Groups[1].Value)
                {
                    case "Availability":
                        HandleAvailabilityCommand(hotels, bookings, parameters);
                        break;
                    case "Search":
                        HandleSearchCommand(input, hotels, bookings);
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
        // TODO: Add return code: 0 for success, 1 for error
    }

    // TODO: Move it to separate class/method?
    private static void HandleAvailabilityCommand( List<Hotel> hotels, List<Booking> bookings, params string[] parameters)
    {
        if (parameters.Length != 3)
        {
            Console.WriteLine("Provide full parameters lists and try again");
            return;
        }

        var hotelId = parameters[0];

        var dateRange = parameters[1];
        DateTime startDate, endDate;
        if (dateRange.Contains('-'))
        {
            var dates = dateRange.Split('-');
            // TODO: Move parsing to a separate method?
            if(!DateTime.TryParseExact(dates[0], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
            {
                Console.WriteLine("Invalid date range");
                return;
            }

            if (!DateTime.TryParseExact(dates[1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
            {
                Console.WriteLine("Invalid date range");
                return;
            }

            if (startDate > endDate)
            {
                Console.WriteLine("Start date cannot be after end date");
                return;
            }
        }
        else
        {
            if (!DateTime.TryParseExact(dateRange, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
            {
                Console.WriteLine("Invalid date range");
                return;
            }
            endDate = startDate;
        }

        var roomType = parameters[2];

        Console.WriteLine(AvailabilityService.FindAvailableRoomsCount(hotelId, startDate, endDate, roomType, hotels, bookings));
    }

    private static void HandleSearchCommand(string input, List<Hotel> hotels, List<Booking> bookings)
    {
        var parts = input.Split(new[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
        string hotelId = parts[1].Trim();
        int daysAhead = int.Parse(parts[2].Trim());
        string roomType = parts[3].Trim();

        var hotel = hotels.FirstOrDefault(h => h.Id == hotelId);
        if (hotel == null)
        {
            Console.WriteLine("Hotel not found");
            return;
        }

        DateTime startDate = DateTime.Today;
        DateTime endDate = startDate.AddDays(daysAhead);

        var availableRooms = hotel.Rooms.Where(r => r.RoomType == roomType).ToList();
        var availabilityRanges = new List<string>();

        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var bookedRooms = bookings.Where(b => b.HotelId == hotelId && b.RoomType == roomType &&
                                                  b.Arrival <= date && b.Departure > date)
                                       .Count();

            int availability = availableRooms.Count - bookedRooms;
            if (availability > 0)
            {
                availabilityRanges.Add($"({date.ToString("yyyyMMdd")}-{date.AddDays(1).ToString("yyyyMMdd")}, {availability})");
            }
        }

        Console.WriteLine(string.Join(", ", availabilityRanges));
    }
}