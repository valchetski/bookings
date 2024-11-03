using Bookings.Models;
using FluentAssertions;
using static Bookings.IntegrationTests.TestsInfrastructure.TestsHelper;

namespace Bookings.IntegrationTests;

public class AvailabilityTests
{
    [Theory]
    [InlineData("Availability(H1)\n")]
    [InlineData("Availability(H1,)\n")]
    [InlineData("Availability(H1,20240901)\n")]
    [InlineData("Availability(H1,20240901,)\n")]
    public void ShouldShowError_WhenArgumentsListIsIncomplete(string incompleteAvailabilityCommand)
    {
        // Arrange
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", BookingsFilePath };

        // Act
        var output = CaptureConsoleOutput(() =>
        {
            Console.SetIn(new StringReader(incompleteAvailabilityCommand));
            Program.Main(args);
        });

        // Assert
        output.Should().Contain("Provide full parameters lists and try again");
    }

    [Fact]
    public void ShouldShowHotelNotFound_WhenHotelDoesNotExist()
    {
        // Arrange
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", BookingsFilePath };

        // Act
        var output = CaptureConsoleOutput(() =>
        {
            Console.SetIn(new StringReader("Availability(dont_exist, 20230101-20230102, SGL)\n"));
            Program.Main(args);
        });

        // Assert
        output.Should().Contain("Hotel not found");
    }

    [Theory]
    [InlineData("not a date")]
    [InlineData("1984")]
    [InlineData("20230101-not_a_date")]
    [InlineData("not_a_date-20230101")]
    public void ShouldShowError_WhenDateRangeIsInvalid(string invalidDateRange)
    {
        // Arrange
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", BookingsFilePath };

        // Act
        var output = CaptureConsoleOutput(() =>
        {
            Console.SetIn(new StringReader($"Availability(1, {invalidDateRange}, SGL)\n"));
            Program.Main(args);
        });

        // Assert
        output.Should().Contain("Invalid date range");
    }

    [Fact]
    public void ShouldShowError_WhenStartDateIsAfterEndDate()
    {
        // Arrange
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", BookingsFilePath };

        // Act
        var output = CaptureConsoleOutput(() =>
        {
            Console.SetIn(new StringReader("Availability(1, 20230102-20230101, SGL)\n"));
            Program.Main(args);
        });

        // Assert
        output.Should().Contain("Start date cannot be after end date");
    }

    [Fact]
    public void ShouldShowAvailability_WhenAllRoomsAreAvailable()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking
            {
                HotelId = "H1",
                Arrival = new DateTime(2024, 1, 1),
                Departure = new DateTime(2024, 1, 2),
                RoomType = "SGL"
            }
        };
        var bookingsFile = CreateTempFile(bookings);
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", bookingsFile };

        // Act
        var output = CaptureConsoleOutput(() =>
        {
            Console.SetIn(new StringReader("Availability(H1, 20240104-20240105, SGL)\n"));
            Program.Main(args);
        });

        // Assert
        output.Should().Contain("2");
    }
}
