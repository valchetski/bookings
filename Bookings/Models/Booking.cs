using System.Text.Json.Serialization;

namespace Bookings.Models;

public class Booking
{
    public string HotelId { get; set; }
    public DateTime Arrival { get; set; }
    public DateTime Departure { get; set; }
    public string RoomType { get; set; }
    public string RoomRate { get; set; }
}