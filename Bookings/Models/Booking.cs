namespace Bookings.Models;

public class Booking
{
    public required string HotelId { get; set; }
    public DateTime Arrival { get; set; }
    public DateTime Departure { get; set; }
    public required string RoomType { get; set; }
    public string? RoomRate { get; set; }
}