namespace Api.Utils
{
    public class DateTimeUtils
    {
        public static long ToUnixTimestamp(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalSeconds;
        }

        public static DateTime FromUnixTimestamp(long timestamp)
        {
            return DateTime.UnixEpoch.AddSeconds(timestamp);
        }
    }
}