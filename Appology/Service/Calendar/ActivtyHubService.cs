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
        Task<bool> AddAsync(ActivityHub activity);
        Task<Dictionary<ActivityTagGroup, IList<HoursWorkedInTag>>> GetActivities(User user, BaseDateFilter dateFilter, bool cacheRemove = false);
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

        public async Task<bool> AddAsync(ActivityHub activity)
        {
            cache.Remove($"{cachePrefix}.{nameof(GetAllByUserIdAsync)}");
            return await activtyHubRepository.AddAsync(activity);
        }

        public async Task<Dictionary<ActivityTagGroup, IList<HoursWorkedInTag>>> GetActivities(User user, BaseDateFilter dateFilter, bool cacheRemove = false)
        {
            var userCalendarIds = (await userService.UserCalendars(user.UserID)).Select(x => x.Id).ToArray();

            var eventRequest = new RequestEventDTO
            {
                CalendarIds = userCalendarIds,
                DateFilter = dateFilter
            };

            var events = (await eventService.GetEventsAsync(eventRequest, cacheRemove))
                .Where(x => (x.UserID == user.UserID || x.InviteeIdsList.Contains(user.UserID)) && !x.Reminder && x.TagID.HasValue && x.WeeklyHourlyTarget != -1)
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
                    WeeklyHourlyTarget = x.WeeklyHourlyTarget,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate
                });

            var getHubActivities = await GetAllByUserIdAsync(user.UserID, dateFilter);
            var activityHub = events.Concat(getHubActivities);

            var eventsOverview = new Dictionary<ActivityTagGroup, IList<HoursWorkedInTag>>();

            if (activityHub != null && activityHub.Any())
            {
                var hoursWorkedInTag = new List<HoursWorkedInTag>();

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


                        double minutesWorked = e.Sum(x => x.IsEvent ? x.EndDate.HasValue ? DateUtils.MinutesBetweenDates(x.EndDate.Value, x.StartDate.Value) : 1440 : x.Minutes);

                        if (minutesWorked > 0)
                        {
                            var additionalInfo = ActivityAdditionalText(minutesWorked, dateFilter, isFlexTag: tag.Subject == "Flex");
                            string text;

                            if (dateFilter.Frequency == DateFrequency.Upcoming)
                            {
                                string multipleEvents = e.Count() > 1 ? "have upcoming events totalling" : "have an upcoming event for";
                                text = string.Format($"{userName} {multipleEvents} {DateUtils.HoursDurationFromMinutes(minutesWorked)} with {tag.Subject}.");
                            }
                            else
                            {
                                text = string.Format($"{userName} spent {DateUtils.HoursDurationFromMinutes(minutesWorked)} {additionalInfo.Text} with {tag.Subject}.");
                            }



                            hoursWorkedInTag.Add(new HoursWorkedInTag
                            {
                                TagGroupId = tagGroup.Key.TagGroupId,
                                Text = text,
                                TotalMinutes = minutesWorked,
                                MultiUsers = multiUser,
                                Color = tag.ThemeColor,
                                Avatars = inviteeAvatars.Distinct().ToList(),
                                ActivityTag = multiUser ? "fa-user-friends" : "fa-tag",
                                ProgressBarWeeklyHours = ProgressBarWeeklyHours(additionalInfo.WeeklyHours, tag.WeeklyHourlyTarget)
                            });
                        }

                    }

                    eventsOverview.Add(new ActivityTagGroup
                    {
                        TagGroupdId = tagGroup.Key.TagGroupId,
                        TagGroupName = tagGroup.Key.TagGroupName
                    }, hoursWorkedInTag);


                    foreach (var group in eventsOverview.Keys)
                    {
                        var groupEventActivity = hoursWorkedInTag.Where(x => x.TagGroupId == group.TagGroupdId);

                        if (groupEventActivity.Count() > 1)
                        {
                            double minutesSpent = groupEventActivity.Sum(x => x.TotalMinutes);

                            group.Text = DateUtils.HoursDurationFromMinutes(minutesSpent);

                            if (minutesSpent > 0)
                            {
                                group.Text += ActivityAdditionalText(minutesSpent, dateFilter).Text;
                            }
                        }
                    }
                }
            }

            return eventsOverview;
        }

        private async Task<IEnumerable<ActivityHub>> GetAllByUserIdAsync(Guid userId, BaseDateFilter dateFilter)
        {
            dateFilter.DateField = "Date";

            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(GetAllByUserIdAsync)}",
                async () => await activtyHubRepository.GetAllByUserIdAsync(userId, dateFilter)
            );
        }

        private ProgressBarWeeklyHours ProgressBarWeeklyHours(int actualHours, int targetHours)
        {
            int progressBarPercentage = (int)Math.Round((double)(100 * actualHours) / targetHours);
            string progressBarColor = "";

            if (progressBarPercentage < 50)
            {
                progressBarColor = "bg-danger";
            }
            else if (progressBarPercentage >= 50 && progressBarPercentage < 75)
            {
                progressBarColor = "bg-warning";
            }
            else if (progressBarPercentage >= 75 && progressBarPercentage < 100)
            {
                progressBarColor = "bg-info";
            }
            else if (progressBarPercentage >= 100)
            {
                progressBarColor = "bg-success";
            }

            return new ProgressBarWeeklyHours
            {
                TargetWeeklyHours = targetHours,
                ActualWeeklyHours = actualHours,
                ProgressBarPercentage = progressBarPercentage,
                ProgressBarColor = progressBarColor
            };
        }


        private (string Text, int WeeklyHours) ActivityAdditionalText(double minutesSpent, BaseDateFilter dateFilter, bool isFlexTag = false)
        {
            int hoursFromMinutes = DateUtils.GetHoursFromMinutes(minutesSpent);
            int? monthsBetween = DateUtils.MonthsBetweenRanges(dateFilter);

            if (monthsBetween.HasValue && monthsBetween.Value != 0)
            {
                int averageWeeklyHours = (hoursFromMinutes / monthsBetween.Value / 4);

                if (averageWeeklyHours > 0)
                {
                    if (isFlexTag)
                    {
                        double averageWeeklyEarning = (hoursFromMinutes * 14 * 1.15) / monthsBetween.Value / 4;
                        string earning = Utils.ToCurrency((decimal)averageWeeklyEarning);

                        return ($" averaging {averageWeeklyHours } hour{(averageWeeklyHours > 1 ? "s" : "")}, {earning} a week", averageWeeklyHours);
                    }
                    else
                    {

                        return ($" averaging {averageWeeklyHours} hour{(averageWeeklyHours > 1 ? "s" : "")} a week", averageWeeklyHours);
                    }
                }
            }
            else
            {
                if (isFlexTag)
                {
                    return ($" earning approx £{hoursFromMinutes * 14 * 1.15}", 0);
                }
            }

            return ("", 0);
        }

    }
}
