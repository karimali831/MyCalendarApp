using MyCalendar.DTOs;
using MyCalendar.Enums;
using MyCalendar.Helpers;
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
         
        [HttpPost]
        public async Task<JsonResult> Get(int[] calendarId)
        {
            await BaseViewModel(new MenuItem { Home = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            var events = await eventService.GetAllAsync(baseVM.User, calendarId);
            var dto = events.Select(b => EventDTO.MapFrom(b)).ToList();

            var activeEvents = await eventService.GetCurrentActivityAsync();
            var currentActivity = await CurrentUserActivity(activeEvents, baseVM.User.UserID);

            return new JsonResult { Data = new { events = dto, currentActivity }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public async Task<JsonResult> Save(DTOs.EventVM e)
        {
            var status = false;
            var user = await GetUser();

            if (user == null)
            {
                return new JsonResult { Data = new { status, responseText = "You are no longer logged in because the session expired" } };
            }

            if (e.EventID != Guid.Empty)
            {
                var eventCreatedUserId = (await eventService.GetAsync(e.EventID)).UserID;
                if (user.UserID != eventCreatedUserId)
                {
                    return new JsonResult { Data = new { status, responseText = "You're attempting to update an event that you did not create" } };
                }
            }

            if (e.TagID == Guid.Empty && string.IsNullOrEmpty(e.Description))
            {
                return new JsonResult { Data = new { status, responseText = "Either tag or description must be entered" } };
            }

            e.UserID = (await GetUser()).UserID;
            
            status = await eventService.SaveEvent(e);
            return new JsonResult { Data = new { status } };
        }

        [HttpPost]
        public async Task<JsonResult> Delete(Guid eventID)
        {
            var user = await GetUser();
            if (user == null)
            {
                return new JsonResult { Data = new { status = false, responseText = "You are not logged-in" } };
            }

            // check the event was created by the user
            var e = await eventService.GetAsync(eventID);
            var userId = (await GetUser()).UserID;

            var status = e.UserID != userId ? false : await eventService.DeleteEvent(eventID, e.EventUid);

            return new JsonResult { Data = new { status } };
        }

        public async Task<ActionResult> MultiAdd(int dates = 0)
        {
            await BaseViewModel(new MenuItem { MultiAdd = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            var viewModel = new SchedulerVM 
            { 
                Dates = dates, 
                Calendars =  await eventService.GetUserCalendars(baseVM.User.UserID)
            };

            var scheduler = (SchedulerVM)TempData["scheduler"];

            if (scheduler != null)
            {
                viewModel.CalendarId = scheduler.CalendarId;
                viewModel.Dates = scheduler.Dates;
                viewModel.TagId = scheduler.TagId ?? null;
                viewModel.StartDate = scheduler.StartDate;
                viewModel.EndDate = scheduler.EndDate;
                viewModel.Alarm = scheduler.Alarm;
                viewModel.UpdateStatus = (scheduler.UpdateStatus.UpdateResponse, scheduler.UpdateStatus.UpdateMsg);
            }

            return View("MultiAdd", viewModel);
        }


        [HttpPost]
        public async Task<ActionResult> MultiAdd(SchedulerVM model)
        {
            var user = await GetUser();
            var events = new Dictionary<int, Model.EventDTO>();
            var userId = (await GetUser()).UserID;

            (Status? UpdateResponse, string UpdateMsg) status = (null, null);

            int z = 0;
            foreach (var item in model.TagId)
            {
                events.Add(z, new Model.EventDTO { 
                    TagID = item,
                    StartDate = model.StartDate[z],
                    EndDate = model.EndDate[z],
                    Alarm = model.Alarm[z]
                });
                z++;
            }

            model.Events = events.Values.Select(x => new Model.EventDTO
            {
                CalendarId = model.CalendarId,
                UserID = user.UserID,
                TagID = x.TagID ?? null,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Alarm = x.Alarm
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
                status = await eventService.SaveEvents(model.Events.ToList())
                    ? (Status.Success, "Scheduled events has been added to your calendar")
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