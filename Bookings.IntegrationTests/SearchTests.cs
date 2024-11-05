using Bookings.Models;
using FluentAssertions;
using System.Text.Json;
using Xunit.Abstractions;
using static Bookings.IntegrationTests.TestsInfrastructure.TestsHelper;
using static Bookings.DateUtils;

namespace Bookings.IntegrationTests;

public class SearchTests(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("Search(H1)")]
    [InlineData("Search(H1,)")]
    [InlineData("Search(H1,365)")]
    [InlineData("Search(H1,365,)")]
    public void ShouldShowError_WhenArgumentsListIsIncomplete(string incompleteSearchCommand)
    {
        // Arrange
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", BookingsFilePath };

        // Act
        var output = CaptureConsoleOutput(incompleteSearchCommand, args);

        // Assert
        output.Should().Contain("Provide full parameters list and try again");
    }

    [Fact]
    public void ShouldShowHotelNotFound_WhenHotelDoesNotExist()
    {
        // Arrange
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", BookingsFilePath };

        // Act
        var output = CaptureConsoleOutput("Search(NonExistentHotel, 365, SGL)", args);

        // Assert
        output.Should().Contain("Hotel not found");
    }

    [Theory]
    [InlineData("not a number")]
    [InlineData("-365")]
    public void ShouldShowError_WhenNumberOfDaysIsInvalid(string invalidDays)
    {
        // Arrange
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", BookingsFilePath };

        // Act
        var output = CaptureConsoleOutput($"Search(H1, {invalidDays}, SGL)", args);

        // Assert
        output.Should().Contain("Invalid number of days");
    }

    [Fact]
    public void ShouldShowError_WhenRoomTypeIsNotSupportedByTheHotel()
    {
        // Arrange
        var bookings = new List<Booking>();
        var bookingsFile = CreateTempFile(bookings);
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", bookingsFile };
        var days = 365;

        // Act
        var output = CaptureConsoleOutput($"Search(H1, {days}, UNSUPPORTED)", args);

        // Assert
        output.Trim().Should().Be("Room type is not supported by the hotel.");
    }

    [Fact]
    public void ShouldShowResults_WhenAllRoomsAreAvailable()
    {
        // Arrange
        var bookings = new List<Booking>();
        var bookingsFile = CreateTempFile(bookings);
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", bookingsFile };
        var days = 365;

        // Act
        var output = CaptureConsoleOutput($"Search(H1, {days}, SGL)", args);

        // Assert
        var startDate = DateTime.Today;
        var endDate = startDate.AddDays(days);
        output.Trim().Should().Be($"({Format(startDate)}-{Format(endDate)},2)");
    }

    [Fact]
    public void ShouldShowResults_WhenPartiallyAvailable()
    {
        // Arrange
        var today = DateTime.Today;
        var oneReservationEndDate = today.AddDays(1);
        var partiallyBookedStartDate = today.AddDays(4);
        var partiallyBookedEndDate = today.AddDays(5);
        var fullyBookedStartDate = today.AddDays(7);
        var fullyBookedEndDate = today.AddDays(8);
        var daysAhead = 365;

        var bookings = new List<Booking>
        {
            new() {
                HotelId = "H1",
                Arrival = today,
                Departure = oneReservationEndDate,
                RoomType = "SGL"
            },
            new() {
                HotelId = "H1",
                Arrival = partiallyBookedStartDate,
                Departure = partiallyBookedEndDate,
                RoomType = "SGL"
            },
            new() {
                HotelId = "H1",
                Arrival = partiallyBookedStartDate,
                Departure = partiallyBookedEndDate,
                RoomType = "SGL"
            },
            new() {
                HotelId = "H1",
                Arrival = fullyBookedStartDate,
                Departure = fullyBookedEndDate,
                RoomType = "SGL"
            },
            new() {
                HotelId = "H1",
                Arrival = fullyBookedStartDate,
                Departure = fullyBookedEndDate,
                RoomType = "SGL"
            },
            new() {
                HotelId = "H1",
                Arrival = fullyBookedStartDate,
                Departure = fullyBookedEndDate,
                RoomType = "SGL"
            }
        };

        testOutputHelper.WriteLine("Bookings:");
        testOutputHelper.WriteLine(JsonSerializer.Serialize(bookings, new JsonSerializerOptions() { WriteIndented = true }));

        var bookingsFile = CreateTempFile(bookings);
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", bookingsFile };

        // Act
        var output = CaptureConsoleOutput($"Search(H1, {daysAhead}, SGL)", args);

        // Assert
        output.Trim().Should().Be(
            $"({Format(today)}-{Format(oneReservationEndDate)},1), " +
            $"({Format(oneReservationEndDate)}-{Format(partiallyBookedStartDate)},2), " +
            $"({Format(partiallyBookedEndDate)}-{Format(fullyBookedStartDate)},2), " +
            $"({Format(fullyBookedEndDate)}-{Format(today.AddDays(daysAhead))},2)");
    }

    [Fact]
    public void ShouldShowResults_WhenUnavailable()
    {
        // Arrange
        var fullyBookedStartDate = DateTime.Today;
        var fullyBookedEndDate = fullyBookedStartDate.AddDays(1);
        var daysAhead = 1;

        var bookings = new List<Booking>
        {
            new() {
                HotelId = "H1",
                Arrival = fullyBookedStartDate,
                Departure = fullyBookedEndDate,
                RoomType = "SGL"
            },
            new() {
                HotelId = "H1",
                Arrival = fullyBookedStartDate,
                Departure = fullyBookedEndDate,
                RoomType = "SGL"
            }
        };

        testOutputHelper.WriteLine("Bookings:");
        testOutputHelper.WriteLine(JsonSerializer.Serialize(bookings, new JsonSerializerOptions() { WriteIndented = true }));

        var bookingsFile = CreateTempFile(bookings);
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", bookingsFile };

        // Act
        var output = CaptureConsoleOutput($"Search(H1, {daysAhead}, SGL)", args, false);

        // Assert
        output.Trim().Should().BeEmpty();
    }
}