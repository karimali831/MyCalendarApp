﻿using Appology.DTOs;
using Appology.Enums;
using NodaTime;
using NodaTime.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Appology.Helpers
{
    public static class DateUtils
    {
        public static int GetHoursFromMinutes(double minutes)
        {
            int rtnNo = 0;
            TimeSpan span = TimeSpan.FromMinutes(minutes);

            if (span.TotalHours >= 1)
            {
                rtnNo = (int)Math.Floor(span.TotalHours);
            }

            return rtnNo;
        }


        public static string HoursDurationFromMinutes(double minutes)
        {
            string rtnString = "";
            var span = TimeSpan.FromMinutes(minutes);
            double calcMinutes = Math.Round(span.TotalMinutes);
            double calcHours = Math.Floor(span.TotalHours);

            if (calcHours == 0)
            {
                rtnString = $"{calcMinutes} minute{(calcMinutes > 1 ? "s" : "")}";
            }
            else
            {
                rtnString = $"{calcHours} hour{(calcHours > 1 ? "s" : "")}";

                if (span.Minutes > 0)
                {
                    rtnString += $", {span.Minutes} minute{(span.Minutes > 1 ? "s" : "")}";
                }
            }

            return rtnString;
        }

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

        public static double MinutesBetweenDates(DateTime d1, DateTime d2)
        {
            TimeSpan span = (d1 - d2);
            return span.TotalMinutes;
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

        public static DateTime UtcDateTime() => FromTimeZoneToUtc(DateTime());

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

        public static string FilterDateSql(BaseDateFilter dateFilter)
        {
            if (System.DateTime.TryParseExact(dateFilter.Frequency.ToString(), "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime freq))
            {
                var year = System.DateTime.UtcNow.Date >= freq.Date ? System.DateTime.UtcNow.Year : System.DateTime.UtcNow.Year - 1;
                return $"MONTH({dateFilter.DateField}) = {freq.Month} AND YEAR({dateFilter.DateField}) = {year} " +
                       $"AND [{dateFilter.DateField}] < DATEADD(DAY, DATEDIFF(DAY, 0, GETUTCDATE()) + 1, 0)";
            }

            var filteredDate = dateFilter.Frequency switch
            {
                DateFrequency.DateRange => $"{dateFilter.DateField} >= '{dateFilter.FromDateRange.Value:yyyy-MM-dd HH:mm}' AND {dateFilter.DateField} < '{dateFilter.ToDateRange.Value.AddDays(1):yyyy-MM-dd HH:mm}'",
                DateFrequency.Today => $"[{dateFilter.DateField}] >= DATEADD(DAY, DATEDIFF(DAY, 0, GETUTCDATE()), 0) AND [{dateFilter.DateField}] < DATEADD(DAY, DATEDIFF(DAY, 0, GETUTCDATE()), 1)",
                DateFrequency.Yesterday => $"[{dateFilter.DateField}] >= DATEADD(DAY, DATEDIFF(DAY, 1, GETDATE()), 0) AND [{dateFilter.DateField}] < DATEADD(DAY, DATEDIFF(DAY, 1, GETUTCDATE()), 1)",
                DateFrequency.Upcoming => $"[{dateFilter.DateField}] > DATEADD(DAY, DATEDIFF(DAY, 0, GETUTCDATE()), 0)",
                DateFrequency.LastXDays => $"[{dateFilter.DateField}] >= DATEADD(DAY, DATEDIFF(DAY, 0, GETUTCDATE()), - {dateFilter.Interval})",
                DateFrequency.LastXMonths => $"[{dateFilter.DateField}] >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETUTCDATE()) - {dateFilter.Interval}, DAY(GETUTCDATE()) - 1)",
                DateFrequency.CurrentYear => $"YEAR([{dateFilter.DateField}]) = YEAR(GETUTCDATE())",
                DateFrequency.PreviousYear => $"YEAR([{dateFilter.DateField}]) = YEAR(DATEADD(YEAR, -1, GETUTCDATE()))",
                DateFrequency.AllTime => $"[{dateFilter.DateField}] <= GETUTCDATE()",
                _ => "",
            };

            if (dateFilter.UpcomingIncEndDate)
            {
                filteredDate += dateFilter.Frequency == DateFrequency.Upcoming ? "" : " AND EndDate < GETUTCDATE()";
            }

            return filteredDate;
        }

        public static int? MonthsBetweenRanges(BaseDateFilter filter)
        {
            int? fromMonth = null;
            int? toMonth = null;
            int? fromYear = null;
            int? toYear = null;

            switch (filter.Frequency)
            {
                case DateFrequency.AllTime:
                    fromMonth = 07;
                    toMonth = DateTime().Month;
                    fromYear = 2019;
                    toYear = DateTime().Year;
                    break;

                case DateFrequency.CurrentYear:
                    fromMonth = 01;
                    toMonth = DateTime().Month;
                    fromYear = DateTime().Year;
                    toYear = DateTime().Year;
                    break;

                case DateFrequency.PreviousYear:
                    fromMonth = (DateTime().Year - 1) == 2019 ? 07 : 01;
                    toMonth = 12;
                    fromYear = DateTime().Year - 1;
                    toYear = DateTime().Year - 1;
                    break;

                case DateFrequency.DateRange:
                    fromMonth = filter.FromDateRange?.Month;
                    toMonth = filter.ToDateRange?.Month;
                    fromYear = filter.FromDateRange?.Year;
                    toYear = filter.ToDateRange?.Year;
                    break;

                case DateFrequency.LastXMonths:
                    fromMonth = DateTime().AddMonths(-filter.Interval ?? -1).Month;
                    toMonth = DateTime().Month;
                    fromYear = DateTime().AddMonths(-filter.Interval ?? -1).Year;
                    toYear = DateTime().Year;
                    break;
            }

            if (fromMonth.HasValue && toMonth.HasValue && fromYear.HasValue && toYear.HasValue)
            {
                return ((toYear - fromYear) * 12) + toMonth - fromMonth;
            }

            return null;
        }

        public static bool ShowAverage(BaseDateFilter filter)
        {
            if (!filter.Frequency.HasValue)
            {
                return false;
            }

            int minMonths = 2;

            if (filter.Frequency == DateFrequency.DateRange && filter.FromDateRange.HasValue && filter.ToDateRange.HasValue)
            {
                return MonthsBetweenRanges(filter).HasValue && MonthsBetweenRanges(filter).Value > minMonths ? true : false;

            }
            else if (
                filter.Frequency == DateFrequency.AllTime ||
                (filter.Frequency == DateFrequency.CurrentYear && DateTime().Month > minMonths) ||
                (filter.Frequency == DateFrequency.LastXMonths && filter.Interval.HasValue && filter.Interval.Value > minMonths) ||
                filter.Frequency == DateFrequency.PreviousYear)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}