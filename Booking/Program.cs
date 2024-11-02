using System.Globalization;
using System.Text.Json;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: myapp --hotels hotels.json --bookings bookings.json");
            return;
        }

        string hotelsFile = args[1];
        string bookingsFile = args[3];

        List<Hotel> hotels = JsonSerializer.Deserialize<List<Hotel>>(File.ReadAllText(hotelsFile));
        List<Booking> bookings = JsonSerializer.Deserialize<List<Booking>>(File.ReadAllText(bookingsFile));

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
                                              ((b.Arrival <= startDate && b.Departure > startDate) ||
                                               (b.Arrival < endDate && b.Departure >= endDate) ||
                                               (b.Arrival >= startDate && b.Departure <= endDate)))
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

public class Hotel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<RoomType> RoomTypes { get; set; }
    public List<Room> Rooms { get; set; }
}

public class RoomType
{
    public string Code { get; set; }
    public string Description { get; set; }
    public List<string> Amenities { get; set; }
    public List<string> Features { get; set; }
}

public class Room
{
    public string RoomType { get; set; }
    public string RoomId { get; set; }
}

public class Booking
{
    public string HotelId { get; set; }
    public DateTime Arrival { get; set; }
    public DateTime Departure { get; set; }
    public string RoomType { get; set; }
    public string RoomRate { get; set; }
}
