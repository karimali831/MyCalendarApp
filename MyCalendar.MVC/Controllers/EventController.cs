﻿using MyCalendar.DTOs;
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyCalendar.Controllers
{
    public class EventController : UserMvcController
    {
        private readonly IEventService eventService;

        public EventController(IEventService eventService, IUserService userService, IFeatureRoleService featureRoleService) : base(userService, featureRoleService)
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

            var viewModel = new SchedulerVM 
            { 
                Dates = dates, 
                Calendars = await UserCalendars(baseVM.User.UserID),
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

                if (user.CronofyReady == CronofyStatus.AuthenticatedRightsSet)
                {
                    foreach (var e in model.Events)
                    {
                        updateEvents.Add(await eventService.SaveEvent(e));

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
                    return RedirectToRoute(Url.Home(updateResponse: status.UpdateResponse, updateMsg: status.UpdateMsg));

                }
                else
                {
                    return RedirectToRoute(Url.Scheduler());
                }
            }
        }


        public async Task<ActionResult> Overview()
        {
            await BaseViewModel(new MenuItem { Overview = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            string currentMonth = Utils.DateTime().ToString("MMMM");
            DateFrequency currentFrequency = Utils.ParseEnum<DateFrequency>(currentMonth);

            var dateFilter = new DateFilter
            {
                Frequency = DateFrequency.LastXDays,
                Interval = 7
            };

            var hoursWorkedInTag = await eventService.HoursSpentInTag(baseVM.User, dateFilter);

            return View("Overview",
                new OverviewVM
                {
                    Filter = dateFilter,
                    HoursWorkedInTag = hoursWorkedInTag
                });

        }

        public async Task<JsonResult> GetFilteredOverview(DateFrequency frequency, int interval, DateTime? fromDate = null, DateTime? toDate = null)
        {
            await BaseViewModel(new MenuItem { Overview = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            var dateFilter = new DateFilter
            {
                Frequency = frequency,
                Interval = interval,
                FromDateRange = fromDate,
                ToDateRange = toDate
            };

            var hoursWorkedInTag = await eventService.HoursSpentInTag(baseVM.User, dateFilter);
            return new JsonResult { Data = hoursWorkedInTag, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}