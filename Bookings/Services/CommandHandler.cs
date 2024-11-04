using Bookings.Models;
using static Bookings.DateUtils;

namespace Bookings.Services;

internal class CommandHandler(IAvailabilityService availabilityService) : ICommandHandler
{
    public int HandleAvailabilityCommand(List<Hotel> hotels, List<Booking> bookings, params string[] parameters)
    {
        if (parameters.Length != 3)
        {
            throw new BookingsException("Provide full parameters list and try again");
        }

        var hotelId = parameters[0];

        var dateRange = parameters[1];
        DateTime startDate, endDate;
        if (dateRange.Contains('-'))
        {
            var dates = dateRange.Split('-');
            startDate = Parse(dates[0]);
            endDate = Parse(dates[1]);

            if (startDate > endDate)
            {
                throw new BookingsException("Start date cannot be after end date");
            }
        }
        else
        {
            startDate = Parse(dateRange);
            endDate = startDate;
        }

        var roomType = parameters[2];

        return availabilityService.FindAvailableRoomsCount(hotelId, startDate, endDate, roomType, hotels, bookings);
    }

    public string HandleSearchCommand(List<Hotel> hotels, List<Booking> bookings, params string[] parameters)
    {
        if (parameters.Length != 3)
        {
            throw new BookingsException("Provide full parameters list and try again");
        }

        string hotelId = parameters[0];

        if (!int.TryParse(parameters[1], out int daysAhead) || daysAhead < 1)
        {
            throw new BookingsException("Invalid number of days");
        }

        string roomType = parameters[2];

        var startDate = DateTime.Today;
        var endDate = startDate.AddDays(daysAhead);
        var availabilityRanges = availabilityService.FindAvailableRoomsRanges(hotelId, startDate, endDate, roomType, hotels, bookings);

        (DateTime From, DateTime To, int Availability) combinedRange = default;
        var combinedAvailabilityRanges = new List<string>();
        foreach (var availabilityRange in availabilityRanges)
        {
            if (combinedRange.Availability == availabilityRange.Availability)
            {
                combinedRange.To = availabilityRange.To;
            }
            else
            {
                if (combinedRange.Availability > 0)
                {
                    combinedAvailabilityRanges.Add($"({Format(combinedRange.From)}-{Format(combinedRange.To)},{combinedRange.Availability})");
                }
                combinedRange = availabilityRange;
            }
        }

        if (combinedRange.Availability > 0)
        {
            combinedAvailabilityRanges.Add($"({Format(combinedRange.From)}-{Format(combinedRange.To)},{combinedRange.Availability})");
        }

        return string.Join(", ", combinedAvailabilityRanges);
    }
}
