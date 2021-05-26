using Appology.DTOs;
using Appology.Enums;
using Appology.Helpers;
using Appology.MiCalendar.DTOs;
using Appology.MiCalendar.Helpers;
using Appology.MiCalendar.Model;
using Appology.MiCalendar.Repository;
using Appology.Model;
using Appology.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.MiCalendar.Service
{
    public interface IActivityHubService
    {
        Task<ActivityHub> GetAsync(Guid Id);
        Task<bool> DeleteAsync(Guid Id);
        Task<bool> AddAsync(ActivityHub activity);
        Task<ActivityHubStats> GetStats(Guid userId);
        Task<Dictionary<ActivityTagGroup, IList<ActivityTagProgress>>> GetActivities(User user, BaseDateFilter dateFilter, bool cacheRemove = false);
        Task<IEnumerable<ActivityHub>> GetAllByUserIdAsync(Guid userId, BaseDateFilter dateFilter, bool cacheRemove = false);
    }

    public class ActivityHubService : IActivityHubService
    {
        public static readonly string cachePrefix = typeof(ActivityHubService).FullName;
        private readonly IActivityHubRepository activtyHubRepository;
        private readonly IEventService eventService;
        private readonly IUserService userService;
        private readonly ICacheService cache;

        public ActivityHubService(
            IActivityHubRepository activtyHubRepository, 
            IEventService eventService, 
            IUserService userService,
            ICacheService cache)
        {
            this.activtyHubRepository = activtyHubRepository ?? throw new ArgumentNullException(nameof(activtyHubRepository));
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<ActivityHub> GetAsync(Guid Id)
        {
            return await activtyHubRepository.GetAsync(Id);
        }

        public async Task<bool> DeleteAsync(Guid Id)
        {
            cache.Remove($"{cachePrefix}.{nameof(GetAllByUserIdAsync)}");
            return await activtyHubRepository.DeleteAsync(Id);
        }

        public async Task<bool> AddAsync(ActivityHub activity)
        {
            return await activtyHubRepository.AddAsync(activity);
        }

        private async Task<IEnumerable<ActivityHub>> GetActivityEvents(RequestEventDTO eventRequest, Guid userId, bool cacheRemove)
        {
            eventRequest.DateFilter.DateField = "StartDate";

            return (await eventService.GetEventsAsync(eventRequest, cacheRemove))
                .Where(x => (x.UserID == userId || x.InviteeIdsList.Contains(userId)) && !x.Reminder && x.EndDate.HasValue && x.TagID.HasValue && x.TargetUnit == "hours")
                .Select(x => new ActivityHub
                {
                    Id = x.EventID,
                    UserId = x.UserID,
                    TagId = x.TagID.Value,
                    Name = x.Name,
                    Avatar = x.Avatar,
                    TagGroupId = x.TagGroupId,
                    TagGroupName = x.TagGroupName,
                    InviteeIds = x.InviteeIds,
                    IsEvent = true,
                    Subject = x.Subject,
                    ThemeColor = x.ThemeColor,
                    TargetFrequency = x.TargetFrequency,
                    TargetValue = x.TargetValue,
                    TargetUnit = x.TargetUnit,
                    StartDate = DateUtils.FromUtcToLocalTime(x.StartDate),
                    EndDate = DateUtils.FromUtcToLocalTime(x.EndDate.Value)
                });
        }

        public async Task<Dictionary<ActivityTagGroup, IList<ActivityTagProgress>>> GetActivities(User user, BaseDateFilter dateFilter, bool cacheRemove = false)
        {
            var userCalendarIds = (await userService.UserCalendars(user.UserID)).Select(x => x.Id).ToArray();

            var eventRequest = new RequestEventDTO
            {
                CalendarIds = userCalendarIds,
                DateFilter = dateFilter
            };

            var eventsByFilter = await GetActivityEvents(eventRequest, user.UserID, cacheRemove);
            var stats = await GetStats(user.UserID);

            var getHubActivities = await GetAllByUserIdAsync(user.UserID, dateFilter, cacheRemove);
            var activityHub = eventsByFilter.Concat(getHubActivities);

            var eventsOverview = new Dictionary<ActivityTagGroup, IList<ActivityTagProgress>>();

            if (activityHub != null && activityHub.Any())
            {
                var ActivityTagProgress = new List<ActivityTagProgress>();

                foreach (var tagGroup in activityHub.GroupBy(x => new { x.TagGroupId, x.TagGroupName }))
                {
                    foreach (var e in tagGroup.Where(x => x.TagGroupId == tagGroup.Key.TagGroupId).GroupBy(x => x.TagId))
                    {
                        var tag = e.FirstOrDefault(x => x.TagId == e.Key);
                        string userName = "You";
                        bool multiUser = false;

                        var inviteeAvatars = new List<string>();

                        if (tag.InviteeIdsList.Any())
                        {
                            inviteeAvatars.Add(CalendarUtils.AvatarSrc(user.UserID, user.Avatar, user.Name));

                            var inviteeList = new List<string>();
                            var invitees = await userService.GetCollaboratorsAsync(tag.InviteeIdsList);

                            foreach (var invitee in invitees)
                            {
                                if (invitee.CollaboratorId != user.UserID)
                                {
                                    inviteeList.Add(invitee.Name);
                                    inviteeAvatars.Add(CalendarUtils.AvatarSrc(invitee.CollaboratorId, invitee.Avatar, invitee.Name));
                                }
                                else if (tag.UserId != user.UserID)
                                {
                                    inviteeList.Add(tag.Name);
                                    inviteeAvatars.Add(CalendarUtils.AvatarSrc(tag.UserId, tag.Avatar, tag.Name));
                                }
                            }

                            userName += ", " + string.Join(", ", inviteeList.Distinct());
                            multiUser = true;
                        }


                        double value = e.Sum(x => x.IsEvent ? x.EndDate.HasValue ? DateUtils.MinutesBetweenDates(x.EndDate.Value, x.StartDate.Value) : 1440 : x.Value);

                        if (value > 0 && tag.TargetFrequency.HasValue)
                        {
                            var additionalInfo = ActivityAdditionalText(value, tag.TargetUnit, tag.TargetFrequency.Value, dateFilter, isGroup: false, tag.Subject);
                            string text;

                            if (dateFilter.Frequency == DateFrequency.Upcoming)
                            {
                                string multipleEvents = e.Count() > 1 ? "have upcoming events totalling" : "have an upcoming event for";
                                text = string.Format($"{userName} {multipleEvents} {DateUtils.HoursDurationFromMinutes(value)} with {tag.Subject}.");
                            }
                            else
                            {
                                if (tag.TargetUnit == "hours")
                                {
                                    text = string.Format($"{userName} spent {DateUtils.HoursDurationFromMinutes(value)} {additionalInfo.Text} with {tag.Subject}.");
                                }
                                else
                                {
                                    text = string.Format($"{userName} completed {value} {tag.TargetUnit} {additionalInfo.Text} with {tag.Subject}.");
                                }
                            }

                            if (tag.TargetValue.HasValue && tag.TargetFrequency.HasValue)
                            {
                                double previousSecondMonthTotalValue = stats.PrevSecondMonth.FirstOrDefault(x => x.TagId == tag.TagId)?.TotalValue ?? 0;
                                double previousMonthTotalValue = stats.PrevMonth.FirstOrDefault(x => x.TagId == tag.TagId)?.TotalValue ?? 0;
                                double thisPeriodTotalValue;
                                double lastPeriodTotalValue = 0;

                                if (tag.TargetFrequency == TimeFrequency.Monthly)
                                {
                                    thisPeriodTotalValue = stats.ThisMonth.FirstOrDefault(x => x.TagId == tag.TagId)?.TotalValue ?? 0;
                                }
                                else
                                {
                                    thisPeriodTotalValue = stats.ThisWeek.FirstOrDefault(x => x.TagId == tag.TagId)?.TotalValue ?? 0;
                                    lastPeriodTotalValue = stats.LastWeek.FirstOrDefault(x => x.TagId == tag.TagId)?.TotalValue ?? 0;
                                }


                                bool lastPeriodSuccess;
                                bool previousSecondMonthSuccess;
                                bool previousMonthSuccess;

                                bool reverse = tag.TargetValue.Value < 0;
                                double actualTargetValue = reverse ? tag.TargetValue.Value * -1 : tag.TargetValue.Value;

                                double targetValueByFrequency = actualTargetValue;
                                double targetValueByMonth = actualTargetValue;

                                if (tag.TargetFrequency == TimeFrequency.Monthly)
                                {
                                    targetValueByFrequency /= 4;
                                }
                                else if (tag.TargetFrequency == TimeFrequency.Daily)
                                {
                                    targetValueByFrequency /= 4 / 7;
                                    targetValueByMonth *= 7 * 4;
                                }
                                else if (tag.TargetFrequency == TimeFrequency.Weekly)
                                {
                                    targetValueByMonth *= 4;
                                }

                                if (!reverse)
                                {
                                    previousSecondMonthSuccess = previousSecondMonthTotalValue >= targetValueByMonth;
                                    previousMonthSuccess = previousMonthTotalValue >= targetValueByMonth;
                                    lastPeriodSuccess = lastPeriodTotalValue != 0 && lastPeriodTotalValue >= targetValueByFrequency;
                                }
                                else
                                {
                                    previousSecondMonthSuccess = previousSecondMonthTotalValue <= targetValueByMonth;
                                    previousMonthSuccess = previousMonthTotalValue <= targetValueByMonth;
                                    lastPeriodSuccess = lastPeriodTotalValue != 0 && lastPeriodTotalValue <= targetValueByFrequency;
                                }

                                ActivityTagProgress.Add(new ActivityTagProgress
                                {
                                    TagGroupId = tagGroup.Key.TagGroupId,
                                    TargetUnit = tag.TargetUnit,
                                    Text = text,
                                    Value = value,
                                    MultiUsers = multiUser,
                                    Color = tag.ThemeColor,
                                    Avatars = inviteeAvatars.Distinct().ToList(),
                                    ActivityTag = multiUser ? "fa-user-friends" : "fa-tag",
                                    ProgressBar = ProgressBar(additionalInfo.Hours, tag.TargetFrequency.Value, actualTargetValue, tag.TargetUnit, reverse),
                                    PreviousMonthTotalValue = previousMonthTotalValue,
                                    PreviousSecondMonthTotalValue = previousSecondMonthTotalValue,
                                    ThisPeriodTotalValue = thisPeriodTotalValue,
                                    LastPeriodTotalValue = lastPeriodTotalValue,
                                    PreviousMonthSuccess = previousMonthSuccess,
                                    PreviousSecondMonthSuccess = previousSecondMonthSuccess,
                                    LastPeriodSuccess = lastPeriodSuccess
                                });
                            }
                        }

                    }

                    eventsOverview.Add(new ActivityTagGroup
                    {
                        TagGroupdId = tagGroup.Key.TagGroupId,
                        TagGroupName = tagGroup.Key.TagGroupName
                    }, ActivityTagProgress);


                    foreach (var group in eventsOverview.Keys)
                    {
                        var groupEventActivity = ActivityTagProgress.Where(x => x.TagGroupId == group.TagGroupdId && x.TargetUnit == "hours");

                        if (groupEventActivity.Count() > 1)
                        {
                            double value = groupEventActivity.Sum(x => x.Value);

                            group.Text = $"Totalling {DateUtils.HoursDurationFromMinutes(value)}";

                            if (value > 0)
                            {
                                group.Text += $" {ActivityAdditionalText(value, "hours", TimeFrequency.Weekly, dateFilter, isGroup: true).Text}";
                            }
                        }
                    }
                }
            }

            return eventsOverview;
        }

        private IList<ActivityHubStatsMonth> GetStatsFilter(IEnumerable<ActivityHubStatsMonth> stats)
        {
            return stats
                .Select(x => {
                    x.TotalValue = x.TargetUnit == "hours" ? DateUtils.GetHoursFromMinutes(x.TotalValue) : x.TotalValue;
                    return x;
                })
                .ToList();
        }

        public async Task<ActivityHubStats> GetStats(Guid userId)
        {
            string thisMonth = DateUtils.DateTime().ToString("MMMM");
            string prevMonth = DateUtils.DateTime().AddMonths(-1).ToString("MMMM");
            string prevSecondMonth = DateUtils.DateTime().AddMonths(-2).ToString("MMMM");

            var prevMonthDateFilter = new ActivityHubDateFilter { Frequency = Utils.ParseEnum<DateFrequency>(prevMonth) };
            var prevSecondMonthFilter = new ActivityHubDateFilter { Frequency = Utils.ParseEnum<DateFrequency>(prevSecondMonth)};
            var thisWeekFilter = new ActivityHubDateFilter { Frequency = DateFrequency.DayOfWeek, Interval = 0 };
            var lastWeekFilter = new ActivityHubDateFilter { Frequency = DateFrequency.DayOfWeek, Interval = -7 };
            var thisMonthFilter = new ActivityHubDateFilter { Frequency = Utils.ParseEnum<DateFrequency>(thisMonth) };

            var prevMonthStats = GetStatsFilter(await activtyHubRepository.GetStats(userId, prevMonthDateFilter));
            var prevSecondMonthStats = GetStatsFilter(await activtyHubRepository.GetStats(userId, prevSecondMonthFilter));
            var thisWeekStats = GetStatsFilter(await activtyHubRepository.GetStats(userId, thisWeekFilter));
            var lastWeekStats = GetStatsFilter(await activtyHubRepository.GetStats(userId, lastWeekFilter));
            var thisMonthStats = GetStatsFilter(await activtyHubRepository.GetStats(userId, thisMonthFilter));

            return new ActivityHubStats
            {
                PrevMonth = prevMonthStats,
                PrevSecondMonth = prevSecondMonthStats,
                ThisWeek = thisWeekStats,
                LastWeek = lastWeekStats,
                ThisMonth = thisMonthStats
            };
        }

        public async Task<IEnumerable<ActivityHub>> GetAllByUserIdAsync(Guid userId, BaseDateFilter dateFilter, bool cacheRemove = false)
        {
            dateFilter.DateField = "Date";

            string cacheName = $"{cachePrefix}.{nameof(GetAllByUserIdAsync)}";

            if (cacheRemove)
            {
                cache.Remove(cacheName);
            }

            return await cache.GetAsync(cacheName, async () => await activtyHubRepository.GetAllByUserIdAsync(userId, dateFilter));
        }

        private ProgressBar ProgressBar(double actualHours, TimeFrequency targetFrequency, double targetValue, string targetUnit, bool reverse)
        {
            int progressBarPercentage = (int)Math.Round((double)(100 * actualHours) / targetValue);
            string progressBarColor = "";

            if (progressBarPercentage <= 10)
            {
                progressBarColor = reverse ? "bg-success" : "bg-danger";
            }
            else if (progressBarPercentage > 10 && progressBarPercentage <= 30)
            {
                progressBarColor = reverse ? "bg-info" : "bg-danger";
            }
            else if (progressBarPercentage > 30 && progressBarPercentage <= 50)
            {
                progressBarColor = reverse ? "" : "bg-warning";
            }
            else if (progressBarPercentage > 50 && progressBarPercentage <= 70)
            {
                progressBarColor = reverse ? "bg-warning" : "";
            }
            else if (progressBarPercentage > 70 && progressBarPercentage <= 90)
            {
                progressBarColor = reverse ? "bg-danger" : "bg-info";
            }
            else if (progressBarPercentage > 90)
            {
                progressBarColor = reverse ? "bg-danger" : "bg-success";
            }

            return new ProgressBar
            {
                TargetFrequency = targetFrequency,
                TargetUnit = targetUnit,
                TargetValue = targetValue,
                ActualValue = Math.Round(actualHours, 2),
                ProgressBarPercentage = progressBarPercentage,
                ProgressBarColor = progressBarColor
            };
        }

        private string CalculateApproxAverageEarning(string subject, double value, TimeFrequency frequency, int? monthsBetween)
        {
            if (subject == "Flex" || subject == "Deliveroo")
            {
                var calcApproxEarning = subject switch
                {
                    "Flex" => value * 14 * 1.15,
                    "Deliveroo" => value * 5.50,
                    _ => value,
                };

                double averageEarning = CalculateAverageHoursByFrequency(frequency, calcApproxEarning, monthsBetween.Value);
                return $" earning approx {Utils.ToCurrency((decimal)averageEarning)} {frequency.ToString().ToLower()}";
                //return "";
            }
            return "";
        }

        private double CalculateAverageHoursByFrequency(TimeFrequency frequency, double value, int monthsBetween)
        {
            double rtnVal;

            switch (frequency)
            {
                case TimeFrequency.Daily:
                    rtnVal = value / monthsBetween / 4 / 7;
                    break;

                case TimeFrequency.Weekly:
                    rtnVal = value / monthsBetween / 4;
                    break;

                case TimeFrequency.Monthly:
                    rtnVal = value / monthsBetween;
                    break;

                default:
                    return value / monthsBetween / 4;
            }

            return Math.Round(rtnVal, 1);
        }

        private (string Text, double Hours) ActivityAdditionalText(double value, string unit, TimeFrequency frequency, BaseDateFilter dateFilter, bool isGroup, string subject = null)
        {
            double hoursFromMinutes = DateUtils.GetHoursFromMinutes(value);
            int? monthsBetween = DateUtils.MonthsBetweenRanges(dateFilter);

            if (monthsBetween.HasValue && monthsBetween.Value != 0)
            {
                if (unit == "hours")
                {
                    double averageHours = CalculateAverageHoursByFrequency(frequency, hoursFromMinutes, monthsBetween.Value);

                    if (averageHours > 0)
                    {
                        string averaging = isGroup ?
                            $" averaging {averageHours} hour{(averageHours > 1 ? "s" : "")} {frequency.ToString().ToLower()}" :
                             CalculateApproxAverageEarning(subject, hoursFromMinutes, frequency, monthsBetween);

                        return (averaging, averageHours);
                    }
                }
                else
                {
                    string averaging = CalculateApproxAverageEarning(subject, value, frequency, monthsBetween);
                    double averageHours = CalculateAverageHoursByFrequency(frequency, value, monthsBetween.Value);
                        
                    return (averaging, averageHours);
                }
            }
            else
            {
                if (subject == "Flex")
                {
                    return ($" earning approx £{hoursFromMinutes * 14 * 1.15}", 0);
                }
                else if (subject == "Deliveroo")
                {
                    return ($" earning approx £{value * 5.50}", 0);
                }
            }
            

            return ("", 0);
        }

    }
}
