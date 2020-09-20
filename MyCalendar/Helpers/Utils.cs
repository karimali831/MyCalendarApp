using NodaTime;
using NodaTime.Extensions;
using System;
using System.Collections.Generic;

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

        public static IList<KeyValuePair<string, string>> TimePresets()
        {
            return new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("05:45-12:00", "5:45am-12pm"),
                new KeyValuePair<string, string>("08:00-17:00", "8am-5pm"),
                new KeyValuePair<string, string>("09:00-17:00", "9am-5pm"),
                new KeyValuePair<string, string>("09:00-18:00", "9am-6pm"),
                new KeyValuePair<string, string>("12:00-20:00", "12pm-8pm"),
                new KeyValuePair<string, string>("17:00-22:00", "5pm-10pm"),
                new KeyValuePair<string, string>("18:00-22:00", "6pm-10pm")
            };
        }

        public static DateTime FromUtcToLocalTime(this DateTime dateTime)
        {
            return dateTime.ToLocalTime();
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
