using Bookings.Models;
using FluentAssertions;
using static Bookings.IntegrationTests.TestsInfrastructure.TestsHelper;

namespace Bookings.IntegrationTests
{
    public class SearchTests
    {
        [Fact]
        public void ShouldShowError_WhenArgumentsListIsIncomplete()
        {
            // Arrange
            var incompleteSearchCommands = new []
            {
                "Search(H1)",
                "Search(H1,)",
                "Search(H1,365)",
                "Search(H1,365,)"
            };
            var args = new string[] { "--hotels", HotelsFilePath, "--bookings", BookingsFilePath };

            foreach (var incompleteSearchCommand in incompleteSearchCommands)
            {
                // Act
                var output = CaptureConsoleOutput(() => 
                {
                    // Simulate the command execution
                    SearchCommand(incompleteSearchCommand, args);
                });

                // Assert
                output.Should().Contain("Provide full parameter lists and try again");
            }
        }

        [Fact]
        public void ShouldShowHotelNotFound_WhenHotelDoesNotExist()
        {
            // Arrange
            var args = new string[] { "--hotels", HotelsFilePath, "--bookings", BookingsFilePath };

            // Act
            var output = CaptureConsoleOutput(() => 
            {
                // Simulate command execution with a non-existent hotel
                Console.SetIn(new StringReader("Search(NonExistentHotel, 365, SGL)"));
                Program.Main(args);
            });

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
            var output = CaptureConsoleOutput(() => 
            {
                // Simulate command execution with an invalid number of days
                Console.SetIn(new StringReader($"Search(H1, {invalidDays}, SGL)"));
                Program.Main(args);
            });

            // Assert
            output.Should().Contain("Invalid number of days");
        }

        [Fact]
        public void ShouldShowError_WhenRoomTypeIsInvalid()
        {
            // Arrange
            var args = new string[] { "--hotels", HotelsFilePath, "--bookings", BookingsFilePath };

            // Act
            var output = CaptureConsoleOutput(() => 
            {
                // Simulate command execution with an invalid room type
                Console.SetIn(new StringReader("Search(H1, 365, INVALID_ROOM_TYPE)"));
                Program.Main(args);
            });

            // Assert
            output.Should().Contain("Room type not found");
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
                // Simulate command execution for room availability
                SearchCommand("Search(H1, 365, SGL)", args);
            });

            // Assert
            output.Should().Contain("365");
        }

        [Fact]
        public void ShouldShowUnavailable_WhenNoRoomsAreAvailable()
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
                },
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
                // Simulate command execution with no available rooms
                SearchCommand("Search(H1, 365, SGL)", args);
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
                new Booking
                {
                    HotelId = "H1",
                    Arrival = new DateTime(2024, 1, 1),
                    Departure = new DateTime(2024, 1, 2),
                    RoomType = "SGL"
                },
                new Booking
                {
                    HotelId = "H1",
                    Arrival = new DateTime(2024, 1, 1),
                    Departure = new DateTime(2024, 1, 2),
                    RoomType = "SGL"
                },
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
                // Simulate command execution with overbooked rooms
                SearchCommand("Search(H1, 365, SGL)", args);
            });

            // Assert
            output.Trim().Should().Be("-1");
        }
    }
}