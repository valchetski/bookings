using Bookings.Models;

namespace Bookings;

// TODO: Maybe avoid static?
internal static class AvailabilityService
{
    //TODO: SOLID is violated here
    public static int FindAvailableRoomsCount(string hotelId, DateTime startDate, DateTime endDate, string roomType, List<Hotel> hotels, List<Booking> bookings)
    {
        var hotel = hotels.FirstOrDefault(h => h.Id == hotelId);
        if (hotel == null)
        {
            throw new BookingsException("Hotel not found");
        }

        var availableRooms = hotel.Rooms.Where(r => r.RoomType == roomType).ToList();
        var bookedRooms = bookings.Count(b => b.HotelId == hotelId && b.RoomType == roomType &&
                                              (b.Arrival <= startDate && b.Departure > startDate ||
                                               b.Arrival < endDate && b.Departure >= endDate ||
                                               b.Arrival >= startDate && b.Departure <= endDate));

        int availability = availableRooms.Count - bookedRooms;
        return availability;
    }
}
