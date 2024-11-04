using Bookings.Models;

namespace Bookings.Services;

internal interface IAvailabilityService
{
    int FindAvailableRoomsCount(string hotelId, DateTime startDate, DateTime endDate, string roomType, List<Hotel> hotels, List<Booking> bookings);

    List<(DateTime From, DateTime To, int Availability)> FindAvailableRoomsRanges(string hotelId, DateTime startDate, DateTime endDate, string roomType, List<Hotel> hotels, List<Booking> bookings);
}
