using Bookings.Models;
using FluentAssertions;
using static Bookings.IntegrationTests.TestsInfrastructure.TestsHelper;

namespace Bookings.IntegrationTests;

public class SearchTests
{
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
                    new Room { RoomType = "SGL", RoomId = "101" },
                    new Room { RoomType = "SGL", RoomId = "102" }
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
                RoomType = "SGL"
            }
        };
        var hotelsFile = CreateTempFile(hotels);
        var bookingsFile = CreateTempFile(bookings);
        var args = new string[] { "--hotels", hotelsFile, "--bookings", bookingsFile };

        // Act
        var output = CaptureConsoleOutput(() =>
        {
            Console.SetIn(new StringReader("Search(1, 7, SGL)\n"));
            Program.Main(args);
        });

        // Assert
        output.Should().Contain("(20230102-20230103, 2)");
    }
}
