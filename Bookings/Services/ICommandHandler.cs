using Bookings.Models;

namespace Bookings.Services;

internal interface ICommandHandler
{
    int HandleAvailabilityCommand(List<Hotel> hotels, List<Booking> bookings, params string[] parameters);

    string HandleSearchCommand(List<Hotel> hotels, List<Booking> bookings, params string[] parameters);
}
