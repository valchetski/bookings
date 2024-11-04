namespace Bookings.Services;

internal interface IFileParser
{
    T? Parse<T>(string file);
}
