using System.Text.Json;
using Bookings.Models;
using FluentAssertions;

namespace Bookings.IntegrationTests;

public class ProgramTests
{
    private string _terminateProgramInput = "\n";

    [Theory]
    [InlineData()]
    [InlineData("--hotels")]
    [InlineData("--hotels", "hotels.json")]
    [InlineData("--hotels", "hotels.json", "--bookings")]
    [InlineData("--hotels", "hotels.json", "--invalid_parameter_name", "bookings.json")]
    [InlineData("--hotels", null, "--invalid_parameter_name", "bookings.json")]
    public void ShouldShowUsage_WhenArgumentsAreInvalid(params string[] args)
    {
        // Arrange
        // Act
        var output = CaptureConsoleOutput(() => Program.Main(args));

        // Assert
        output.Should().Be("Usage: myapp --hotels hotels.json --bookings bookings.json");
    }

    [Theory]
    [InlineData("file_name_without_extension")]
    [InlineData("incorrect_extension.txt")]
    public void ShouldHandleError_WhenHotelsFileNameIsInvalid(string hotelsValue)
    {
        // Arrange
        var args = new string[] { "--hotels", hotelsValue, "--bookings", "any_value.json" };

        // Act
        var output = CaptureConsoleOutput(() => Program.Main(args));

        // Assert
        output.Trim().Should().Be($"File \"{hotelsValue}\" has invalid extension. Only .json files are supported.");
    }

    [Fact]
    public void ShouldHandleError_WhenHotelsFileDoesntExist()
    {
        // Arrange
        var hotelsValue = "dont_exist.json";
        var args = new string[] { "--hotels", hotelsValue, "--bookings", "any_value.json" };

        // Act
        var output = CaptureConsoleOutput(() => Program.Main(args));

        // Assert
        output.Trim()
            .Should().StartWith("Could not find file")
            .And.EndWith(hotelsValue + "'.");
    }

    [Theory]
    [InlineData("file_name_without_extension")]
    [InlineData("incorrect_extension.txt")]
    public void ShouldHandleError_WhenBookingsFileNameIsInvalid(string bookingsValue)
    {
        // Arrange
        var args = new string[] { "--hotels", "./TestsData/hotels.json", "--bookings", bookingsValue };

        // Act
        var output = CaptureConsoleOutput(() => Program.Main(args));

        // Assert
        output.Trim().Should().Be($"File \"{bookingsValue}\" has invalid extension. Only .json files are supported.");
    }

    [Fact]
    public void ShouldHandleError_WhenBookingsFileDoesntExist()
    {
        // Arrange
        var bookingsValue = "dont_exist.json";
        var args = new string[] { "--hotels", "./TestsData/hotels.json", "--bookings", bookingsValue };

        // Act
        var output = CaptureConsoleOutput(() => Program.Main(args));

        // Assert
        output.Trim()
            .Should().StartWith("Could not find file")
            .And.EndWith(bookingsValue + "'.");
    }

    [Theory]
    [InlineData("--hotels", "./TestsData/hotels.json", "--bookings", "./TestsData/bookings.json")]
    [InlineData("--bookings", "./TestsData/bookings.json", "--hotels", "./TestsData/hotels.json")]
    public void ShouldAcceptInput_WhenArgumentsAreInDifferentOrder(params string[] args)
    {
        // Arrange
        // Act
        var output = CaptureConsoleOutput(() =>
        {
            Console.SetIn(new StringReader(_terminateProgramInput));
            Program.Main(args);
        });

        // Assert
        output.Should().Be(string.Empty, "We've send input args and terminated program with sending empty line");
    }

    [Fact]
    public void ShouldShowHotelNotFound_WhenHotelDoesNotExist()
    {
        // Arrange
        var hotels = new List<Hotel>();
        var bookings = new List<Booking>();
        var hotelsFile = CreateTempFile(hotels);
        var bookingsFile = CreateTempFile(bookings);
        var args = new string[] { "--hotels", hotelsFile, "--bookings", bookingsFile };

        // Act
        var output = CaptureConsoleOutput(() =>
        {
            Console.SetIn(new StringReader("Availability(1, 20230101-20230102, Deluxe)\n"));
            Program.Main(args);
        });

        // Assert
        output.Should().Contain("Hotel not found");
    }

    [Fact]
    public void ShouldShowAvailability_WhenRoomsAreAvailable()
    {
        // Arrange
        var hotels = new List<Hotel>
        {
            new Hotel
            {
                Id = "1",
                Rooms = new List<Room>
                {
                    new Room { RoomType = "Deluxe", RoomId = "101" },
                    new Room { RoomType = "Deluxe", RoomId = "102" }
                }
            }
        };
        var bookings = new List<Booking>
        {
            new Booking
            {
                HotelId = "1",
                Arrival = new DateTime(2023, 1, 1),
                Departure = new DateTime(2023, 1, 2),
                RoomType = "Deluxe"
            }
        };
        var hotelsFile = CreateTempFile(hotels);
        var bookingsFile = CreateTempFile(bookings);
        var args = new string[] { "--hotels", hotelsFile, "--bookings", bookingsFile };

        // Act
        var output = CaptureConsoleOutput(() =>
        {
            Console.SetIn(new StringReader("Availability(1, 20230101-20230102, Deluxe)\n"));
            Program.Main(args);
        });

        // Assert
        output.Should().Contain("1");
    }

    [Fact]
    public void ShouldShowSearchResults_WhenRoomsAreAvailable()
    {
        // Arrange
        var hotels = new List<Hotel>
        {
            new Hotel
            {
                Id = "1",
                Rooms = new List<Room>
                {
                    new Room { RoomType = "Deluxe", RoomId = "101" },
                    new Room { RoomType = "Deluxe", RoomId = "102" }
                }
            }
        };
        var bookings = new List<Booking>
        {
            new Booking
            {
                HotelId = "1",
                Arrival = new DateTime(2023, 1, 1),
                Departure = new DateTime(2023, 1, 2),
                RoomType = "Deluxe"
            }
        };
        var hotelsFile = CreateTempFile(hotels);
        var bookingsFile = CreateTempFile(bookings);
        var args = new string[] { "--hotels", hotelsFile, "--bookings", bookingsFile };

        // Act
        var output = CaptureConsoleOutput(() =>
        {
            Console.SetIn(new StringReader("Search(1, 7, Deluxe)\n"));
            Program.Main(args);
        });

        // Assert
        output.Should().Contain("(20230102-20230103, 2)");
    }

    private string CaptureConsoleOutput(Action action)
    {
        var output = new StringWriter();
        Console.SetOut(output);
        action();
        return output.ToString();
    }

    private string CreateTempFile<T>(T data)
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, JsonSerializer.Serialize(data));
        return tempFile;
    }
}
