using System.Globalization;
using System.Text.Json;
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
            List<Hotel> hotels = JsonFileParser.Parse<List<Hotel>>(hotelsFile);
            List<Booking> bookings = JsonFileParser.Parse<List<Booking>>(bookingsFile);

            while (true)
            {
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    break;

                if (input.StartsWith("Availability"))
                {
                    HandleAvailabilityCommand(input, hotels, bookings);
                }
                else if (input.StartsWith("Search"))
                {
                    HandleSearchCommand(input, hotels, bookings);
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

    private static void HandleAvailabilityCommand(string input, List<Hotel> hotels, List<Booking> bookings)
    {
        var parts = input.Split(new[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
        string hotelId = parts[1].Trim();
        string dateRange = parts[2].Trim();
        string roomType = parts[3].Trim();

        DateTime startDate, endDate;
        if (dateRange.Contains('-'))
        {
            var dates = dateRange.Split('-');
            startDate = DateTime.ParseExact(dates[0], "yyyyMMdd", CultureInfo.InvariantCulture);
            endDate = DateTime.ParseExact(dates[1], "yyyyMMdd", CultureInfo.InvariantCulture);
        }
        else
        {
            startDate = DateTime.ParseExact(dateRange, "yyyyMMdd", CultureInfo.InvariantCulture);
            endDate = startDate;
        }

        var hotel = hotels.FirstOrDefault(h => h.Id == hotelId);
        if (hotel == null)
        {
            Console.WriteLine("Hotel not found");
            return;
        }

        var availableRooms = hotel.Rooms.Where(r => r.RoomType == roomType).ToList();
        var bookedRooms = bookings.Where(b => b.HotelId == hotelId && b.RoomType == roomType &&
                                              (b.Arrival <= startDate && b.Departure > startDate ||
                                               b.Arrival < endDate && b.Departure >= endDate ||
                                               b.Arrival >= startDate && b.Departure <= endDate))
                                   .Count();

        int availability = availableRooms.Count - bookedRooms;
        Console.WriteLine(availability);
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