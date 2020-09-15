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
            var zonedClock = SystemClock.Instance.InZone(zone);
            //bool isDST = zonedDateTime.IsDaylightSavingTime();
            return zonedClock.GetCurrentZonedDateTime().ToDateTimeUnspecified();
        }

        public static DateTime FromTimeZoneToUtc(this DateTime dt, string timezone = "Europe/London")
        {
            var tz = DateTimeZoneProviders.Tzdb[timezone];
            var local = LocalDateTime.FromDateTime(dt);
            return local.InZoneLeniently(tz).ToDateTimeUtc();
        }
    }
}
