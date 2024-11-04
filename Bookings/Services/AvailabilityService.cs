using Bookings.Models;

namespace Bookings.Services;

internal class AvailabilityService : IAvailabilityService
{
    public int FindAvailableRoomsCount(string hotelId, DateTime startDate, DateTime endDate, string roomType, List<Hotel> hotels, List<Booking> bookings)
    {
        var hotel = hotels.FirstOrDefault(h => h.Id == hotelId)
            ?? throw new BookingsException("Hotel not found");

        if (!hotel.RoomTypes.Any(x => x.Code == roomType))
        {
            throw new BookingsException("Room type is not supported by the hotel.");
        }

        var allRooms = hotel.Rooms.Count(r => r.RoomType == roomType);
        var bookedRooms = bookings.Count(b => b.HotelId == hotelId && b.RoomType == roomType &&
                                              startDate < b.Departure && endDate > b.Arrival);

        return allRooms - bookedRooms;
    }

    public List<(DateTime From, DateTime To, int Availability)> FindAvailableRoomsRanges(string hotelId, DateTime startDate, DateTime endDate, string roomType, List<Hotel> hotels, List<Booking> bookings)
    {
        DateTime nextDate;
        var availabilityRanges = new List<(DateTime From, DateTime To, int Availability)>();

        for (var date = startDate; date < endDate; date = date.AddDays(1))
        {
            nextDate = date.AddDays(1);
            var availability = FindAvailableRoomsCount(hotelId, date, nextDate, roomType, hotels, bookings);
            availabilityRanges.Add((date, nextDate, availability));
        }

        return availabilityRanges;
    }
}
