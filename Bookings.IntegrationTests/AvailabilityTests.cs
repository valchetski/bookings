using System.Globalization;
using System.Runtime.InteropServices;
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
        output.Trim().Should().Be("Provide full parameters list and try again");
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
        output.Trim().Should().Be("Hotel not found");
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
        output.Trim().Should().Be("Invalid date");
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
        output.Trim().Should().Be("Start date cannot be after end date");
    }

    [Fact]
    public void ShouldShowError_WhenRoomTypeIsNotSupported()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new() {
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
            Console.SetIn(new StringReader("Availability(H1, 20240104-20240105, NOT_SUPPORTED)\n"));
            Program.Main(args);
        });

        // Assert
        output.Trim().Should().Be("Room type is not supported by the hotel.");
    }

    [Fact]
    public void ShouldShowAvailability_WhenAllRoomsAreAvailable()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new() {
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
        output.Trim().Should().Be("2");
    }
    
    [Theory]
    [InlineData("20240101", "20240102", "20231231-20240101")]
    [InlineData("20240101", "20240102", "20231231")]
    [InlineData("20240101", "20240102", "20240102-20240103")]
    [InlineData("20240101", "20240102", "20240103")]
    public void ShouldShowAvailability_WhenSomeRoomsAreAvailable(
        string bookingStart,
        string bookingEnd,
        string dateRange)
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new() {
                HotelId = "H1",
                Arrival = DateUtils.Parse(bookingStart),
                Departure = DateUtils.Parse(bookingEnd),
                RoomType = "SGL"
            },
            new() {
                HotelId = "H1",
                Arrival = new DateTime(2023, 1, 1),
                Departure = new DateTime(2025, 1, 2),
                RoomType = "SGL"
            }
        };
        var bookingsFile = CreateTempFile(bookings);
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", bookingsFile };

        // Act
        var output = CaptureConsoleOutput(() =>
        {
            Console.SetIn(new StringReader($"Availability(H1, {dateRange}, SGL)\n"));
            Program.Main(args);
        });

        // Assert
        output.Trim().Should().Be("1");
    }
    
    [Fact]
    public void ShouldShowUnavailable_WhenNoRoomsAreAvailable()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new() {
                HotelId = "H1",
                Arrival = new DateTime(2024, 1, 1),
                Departure = new DateTime(2024, 1, 2),
                RoomType = "SGL"
            },
            new() {
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
            Console.SetIn(new StringReader("Availability(H1, 20240101-20240102, SGL)\n"));
            Program.Main(args);
        });

        // Assert
        output.Trim().Should().Be("0");
    }
    
    [Fact]
    public void ShouldShowOverbooking_WhenOverbooked()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new() {
                HotelId = "H1",
                Arrival = new DateTime(2024, 1, 1),
                Departure = new DateTime(2024, 1, 2),
                RoomType = "SGL"
            },
            new() {
                HotelId = "H1",
                Arrival = new DateTime(2024, 1, 1),
                Departure = new DateTime(2024, 1, 2),
                RoomType = "SGL"
            },
            new() {
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
            Console.SetIn(new StringReader("Availability(H1, 20240101-20240102, SGL)\n"));
            Program.Main(args);
        });

        // Assert
        output.Trim().Should().Be("-1");
    }
    
    [Fact]
    public void ShouldShowOverbooking_WhenSignificantlyOverbooked()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new() {
                HotelId = "H1",
                Arrival = new DateTime(2024, 1, 1),
                Departure = new DateTime(2024, 1, 2),
                RoomType = "SGL"
            },
            new() {
                HotelId = "H1",
                Arrival = new DateTime(2024, 1, 1),
                Departure = new DateTime(2024, 1, 2),
                RoomType = "SGL"
            },
            new() {
                HotelId = "H1",
                Arrival = new DateTime(2024, 1, 1),
                Departure = new DateTime(2024, 1, 2),
                RoomType = "SGL"
            },
            new() {
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
            Console.SetIn(new StringReader("Availability(H1, 20240101-20240102, SGL)\n"));
            Program.Main(args);
        });

        // Assert
        output.Trim().Should().Be("-2");
    }
}
