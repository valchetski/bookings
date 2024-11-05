using FluentAssertions;
using static Bookings.IntegrationTests.TestsInfrastructure.TestsHelper;

namespace Bookings.IntegrationTests;

public class ProgramTests
{
    private readonly string _terminateProgramInput = "\n";

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
        var output = CaptureConsoleOutput(string.Empty, args);

        // Assert
        output.Trim().Should().Be("Usage: Bookings.exe --hotels hotels.json --bookings bookings.json");
    }

    [Theory]
    [InlineData("file_name_without_extension")]
    [InlineData("incorrect_extension.txt")]
    public void ShouldHandleError_WhenHotelsFileNameIsInvalid(string hotelsValue)
    {
        // Arrange
        var args = new string[] { "--hotels", hotelsValue, "--bookings", "any_value.json" };

        // Act
        var output = CaptureConsoleOutput(string.Empty, args);

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
        var output = CaptureConsoleOutput(string.Empty, args);

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
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", bookingsValue };

        // Act
        var output = CaptureConsoleOutput(string.Empty, args);

        // Assert
        output.Trim().Should().Be($"File \"{bookingsValue}\" has invalid extension. Only .json files are supported.");
    }

    [Fact]
    public void ShouldHandleError_WhenBookingsFileDoesntExist()
    {
        // Arrange
        var bookingsValue = "dont_exist.json";
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", bookingsValue };

        // Act
        var output = CaptureConsoleOutput(string.Empty, args);

        // Assert
        output.Trim()
            .Should().StartWith("Could not find file")
            .And.EndWith(bookingsValue + "'.");
    }

    [Theory]
    [InlineData("--hotels", HotelsFilePath, "--bookings", BookingsFilePath)]
    [InlineData("--bookings", BookingsFilePath, "--hotels", HotelsFilePath)]
    public void ShouldAcceptInput_WhenArgumentsAreInDifferentOrder(params string[] args)
    {
        // Arrange
        // Act
        var output = CaptureConsoleOutput(_terminateProgramInput, args);

        // Assert
        output.Trim().Should().Be("To exit, press Enter.", "We've send input args and terminated program with sending empty line");
    }

    [Theory]
    [InlineData("Availability_InvalidCommand(1, 20230101-20230102, SGL)\n")]
    [InlineData("CompletelyInvalid(1, 20230101-20230102, SGL)\n")]
    [InlineData("Availability()\n")]
    [InlineData("Search()\n")]
    public void ShouldShowHelp_WhenCommandNotFound(string invalidCommand)
    {
        // Arrange
        var args = new string[] { "--hotels", HotelsFilePath, "--bookings", BookingsFilePath };

        // Act
        var output = CaptureConsoleOutput(invalidCommand, args);

        // Assert
        output.Should().StartWith("Input is invalid");
    }
}
