using System.Globalization;

namespace Bookings;

public static class DateUtils
{
    public static DateTime Parse(string date, string format = "yyyyMMdd")
    {
        if (!DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
        {
            throw new BookingsException("Invalid date");
        }

        return value;
    }

    public static string Format(DateTime date, string format = "yyyyMMdd")
    {
        return date.ToString(format);
    }
}
