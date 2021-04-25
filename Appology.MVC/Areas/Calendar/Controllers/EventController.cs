using Appology.Controllers;
using Appology.MiCalendar.DTOs;
using Appology.Enums;
using Appology.Helpers;
using Appology.MiCalendar.Enums;
using Appology.MiCalendar.Model;
using Appology.MiCalendar.Service;
using Appology.Service;
using Appology.Website.Areas.MiCalendar.ViewModels;
using Appology.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Configuration;

namespace Appology.Areas.MiCalendar.Controllers
{
    public class EventController : UserMvcController
    {
        private readonly IEventService eventService;

        public EventController(
            IUserService userService,
            IEventService eventService,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
   
        }
         
        public async Task<JsonResult> LastStoredAlarm(Guid tagId)
        {
            var alarm = await eventService.GetLastStoredAlarm(tagId);
            return new JsonResult { Data = alarm ?? "", JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        public async Task<ActionResult> MultiAdd(int dates = 0)
        {
            await BaseViewModel(new MenuItem { MultiAdd = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;
            var userCalendars = await UserCalendars(baseVM.User.UserID, userCreated: true);
                
            var viewModel = new SchedulerVM 
            { 
                Dates = dates, 
                Calendars = userCalendars,
                UserTags = await UserTags(baseVM.User.UserID)
            };

            var scheduler = (SchedulerVM)TempData["scheduler"];

            if (scheduler != null)
            {
                viewModel.CalendarId = scheduler.CalendarId;
                viewModel.Dates = scheduler.Dates;
                viewModel.TagId = scheduler.TagId ?? null;
                viewModel.StartDate = scheduler.StartDate;
                viewModel.EndDate = scheduler.EndDate;
                viewModel.UpdateStatus = (scheduler.UpdateStatus.UpdateResponse, scheduler.UpdateStatus.UpdateMsg);

                if (baseVM.User.CronofyReady == CronofyStatus.AuthenticatedRightsSet)
                {
                    viewModel.Alarm = scheduler.Alarm;
                }
            }

            return View("MultiAdd", viewModel);
        }


        [HttpPost]
        public async Task<ActionResult> MultiAdd(SchedulerVM model)
        {
            var user = await GetUser();
            var events = new Dictionary<int, Event>();
   
            (Status? UpdateResponse, string UpdateMsg) status = (null, null);

            int z = 0;
            foreach (var item in model.TagId)
            {
                events.Add(z, new Event { 
                    TagID = item,
                    StartDate = model.StartDate[z],
                    EndDate = model.EndDate[z],
                    Alarm = user.CronofyReady == CronofyStatus.AuthenticatedRightsSet ? model.Alarm[z] : null
                });
                z++;
            }

            model.Events = events.Values.Select(x => new Event
            {
                CalendarId = model.CalendarId,
                UserID = user.UserID,
                TagID = x.TagID ?? null,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Alarm = user.CronofyReady == CronofyStatus.AuthenticatedRightsSet ? x.Alarm : null
            })
            .Where(x => x.StartDate != null && x.EndDate != null);

            foreach (var e in model.Events)
            {
                if (e.StartDate > e.EndDate)
                {
                    status = (Status.Failed, "Invalid end date");
                }
            }

            model.UpdateStatus = (status.UpdateResponse, status.UpdateMsg);
            TempData["scheduler"] = model;

            if (status.UpdateResponse == Status.Failed)
            {
                return RedirectToRoute(Url.Scheduler());
            }
            else
            {
                var updateEvents = new List<bool>();

                foreach (var e in model.Events)
                {
                    updateEvents.Add(await eventService.SaveEvent(e));

                    if (user.CronofyReady == CronofyStatus.AuthenticatedRightsSet)
                    {
                        if (e.EventUid == null)
                        {
                            //e.EventID = e.EventID != Guid.Empty ? e.EventID : Guid.NewGuid();
                            //e.StartDate = Utils.FromTimeZoneToUtc(e.StartDate);
                            //e.EndDate = e.EndDate.HasValue ? Utils.FromTimeZoneToUtc(e.EndDate.Value) : (DateTime?)null;
                            //e.Reminder = false;

                            await eventService.SaveCronofyEvent(e, user.ExtCalendarRights);
                        }
                    }
                }

                status = updateEvents.All(x => x)
                    ? (Status.Success, "Events successfully added to your calendar")
                    : (Status.Failed, "There was as an issue with adding some or all of your scheduled events to your calendar");

                if (status.UpdateResponse == Status.Success)
                {
                    return RedirectToRoute(Url.Home(status.UpdateResponse, status.UpdateMsg));

                }
                else
                {
                    return RedirectToRoute(Url.Scheduler());
                }
            }
        }


        public async Task<ActionResult> ActivityHub()
        {
            await BaseViewModel(new MenuItem { ActivityHub = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            string currentMonth = DateUtils.DateTime().ToString("MMMM");
            DateFrequency currentFrequency = Utils.ParseEnum<DateFrequency>(currentMonth);

            var dateFilter = new DateFilter
            {
                Frequency = DateFrequency.LastXMonths,
                Interval = 1
            };

            var eventsOverview = await eventService.EventActivityTagGroup(baseVM.User, dateFilter);

            return View("ActivityHub",
                new EventActivityHubVM
                {
                    Filter = dateFilter,
                    EventsOverview = eventsOverview
                });
        }

        public async Task<JsonResult> FilteredActivityHub(DateFrequency frequency, int interval, DateTime? fromDate = null, DateTime? toDate = null)
        {
            await BaseViewModel(new MenuItem { ActivityHub = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            var dateFilter = new DateFilter
            {
                Frequency = frequency,
                Interval = interval,
                FromDateRange = fromDate,
                ToDateRange = toDate
            };

            var eventsOverview = await eventService.EventActivityTagGroup(baseVM.User, dateFilter);

            string html = "";

            foreach (var tagGroup in eventsOverview)
            {
                if (!string.IsNullOrEmpty(tagGroup.Key.TagGroupName))
                {
                    string key = Utils.RemoveSpecialCharacters($"{tagGroup.Key.TagGroupName}{tagGroup.Key.TagGroupdId}");
                    html += $"<div class='list-group'><a href='#{key}' class='list-group-item' data-toggle='collapse'><i class='fas fa-chevron-down'></i> {tagGroup.Key.TagGroupName}";

                    if (tagGroup.Value.Count(x => x.TagGroupId == tagGroup.Key.TagGroupdId) > 1)
                    {
                        html += $"<span class='float-right' style='color: #000; font-size: small'><i class='fas fa-tags'></i> {tagGroup.Key.Text}</span>";
                    }

                    html += $"</a><div class='list-group in collapse show' id='{key}'>";

                    foreach (var e in tagGroup.Value.Where(x => x.TagGroupId == tagGroup.Key.TagGroupdId))
                    {
                        html += $"<div class='list-group-item'><span class='fas {e.ActivityTag}' style='color: {e.Color}'></span> <small> {e.Text}</small>";

                        if (e.ProgressBarWeeklyHours.TargetWeeklyHours != 0 && e.ProgressBarWeeklyHours.ProgressBarPercentage > 0)
                        {
                            html += $"<div class='progress'><div class='progress-bar progress-bar-striped progress-bar-animated {e.ProgressBarWeeklyHours.ProgressBarColor}' role='progressbar' aria-valuenow='{e.ProgressBarWeeklyHours.ActualWeeklyHours}' aria-valuemin='0' aria-valuemax='{e.ProgressBarWeeklyHours.TargetWeeklyHours}' style='width: {e.ProgressBarWeeklyHours.ProgressBarPercentage}%'>{e.ProgressBarWeeklyHours.ProgressBarPercentage}% complete targetting {e.ProgressBarWeeklyHours.TargetWeeklyHours} hours a week</div></div>";
                        }

                        foreach (var avatar in e.Avatars)
                        {
                            if (avatar.Length == 2)
                            {
                                html += $"<p default-avatar='{avatar}' style='width: 24px; height: 24px; float: right'></p>";
                            }
                            else
                            {
                                html += $"<img src='{ConfigurationManager.AppSettings["RootUrl"]}/{avatar}' style='width: 24px; height: 24px; float: right' />";
                            }
                        }

                        html += "</div>";
                    }

                    html += "</div></div>";
                }
            }

            return new JsonResult { Data = Content(html), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}