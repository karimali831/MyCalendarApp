using NodaTime;
using NodaTime.Extensions;
using System;

namespace MyCalendar.Helpers
{
    public static class Utils
    {
        public static string Duration(DateTime d1, DateTime d2)
        {
            string rtnString = "";
            TimeSpan span = (d1 - d2);

            if (span.Minutes < 60 && span.Hours == 0 && span.Days == 0)
            {
                rtnString = $"{span.Minutes} minute{(span.Minutes > 1 ? "s" : "")}";
            }
            else if (span.Hours >= 1 && span.Days == 0)
            {
                rtnString = $"{span.Hours} hour{(span.Hours > 1 ? "s" : "")}";

                if (span.Minutes > 0)
                {
                    rtnString += $", {span.Minutes} minute{(span.Minutes > 1 ? "s" : "")}";
                }
            }
            else if (span.Days >= 1)
            {
                rtnString = $"{span.Days} day{(span.Days > 1 ? "s" : "")}";

                if (span.Hours > 1)
                {
                    rtnString += $", {span.Hours} hour{(span.Hours > 1 ? "s" : "")}";
                }

                if (span.Minutes > 0)
                {
                    rtnString += $", {span.Minutes} minute{(span.Minutes > 1 ? "s" : "")}";
                }
            }

            return rtnString;
        }

        public static DateTime DateTime(string timezone = "Europe/London")
        {
            var zone = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default.ForId(timezone);
            var zonedDateTime = SystemClock.Instance.InZone(zone);
            //bool isDST = zonedDateTime.IsDaylightSavingTime();
            return zonedDateTime.GetCurrentZonedDateTime().ToDateTimeUnspecified();
        }

        public static DateTime FromTimeZoneToUtc(this DateTime dateTime, string timezone = "Europe/London")
        {
            DateTimeZone zone = DateTimeZoneProviders.Tzdb[timezone];
            var localtime = LocalDateTime.FromDateTime(dateTime);
            var zonedtime = localtime.InZoneLeniently(zone);
            return zonedtime.ToInstant().InZone(zone).ToDateTimeUtc();
        }

        public static DateTime FromUtcToTimeZone(this DateTime dateTime, string timezone = "Europe/London")
        {
            IDateTimeZoneProvider timeZoneProvider = DateTimeZoneProviders.Tzdb;
            var utcTimeZone = timeZoneProvider["UTC"];
            var dateTimeFromDb = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
            var zonedDbDateTime = utcTimeZone.AtLeniently(LocalDateTime.FromDateTime(dateTimeFromDb));
            var usersTimezone = timeZoneProvider[timezone];
            var usersZonedDateTime = zonedDbDateTime.WithZone(usersTimezone);
            return usersZonedDateTime.ToDateTimeUnspecified();
        }
    }
}
