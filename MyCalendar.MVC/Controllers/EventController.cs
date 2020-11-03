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
        private readonly ICronofyService cronofyService;
        private readonly ITagService tagService;

        public EventController(IEventService eventService, IUserService userService, ICronofyService cronofyService, ITagService tagService) : base(userService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.cronofyService = cronofyService ?? throw new ArgumentNullException(nameof(cronofyService));
            this.tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        }

        public async Task<JsonResult> Get(Guid? viewingId = null, bool combined = false)
        {
            Guid? viewing = null;
            var user = await GetUser();

            if (combined)
            {
                viewing = null;
            }
            else
            {
                viewing = viewingId != null ? viewingId : user.UserID;
            }

            var events = await eventService.GetAllAsync(user, viewing);
            var dto = events.Select(b => EventDTO.MapFrom(b)).ToList();

            var activeEvents = await eventService.GetCurrentActivityAsync();
            var currentActivity = await CurrentUserActivity(activeEvents, user.UserID);

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

            var status = e.UserID != userId ? false : await eventService.DeleteEvent(eventID);

            return new JsonResult { Data = new { status } };
        }

        public async Task<ActionResult> MultiAdd(int dates = 0)
        {
            await BaseViewModel(new MenuItem { MultiAdd = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            var viewModel = new SchedulerVM { Dates = dates };
            var scheduler = (SchedulerVM)TempData["scheduler"];

            if (scheduler != null)
            {
                viewModel.Dates = scheduler.Dates;
                viewModel.TagID = scheduler.TagID == Guid.Empty ? null : scheduler.TagID;
                viewModel.StartDate = scheduler.StartDate;
                viewModel.EndDate = scheduler.EndDate;
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

            int z = 1;
            foreach (var item in model.StartDate)
            {
                events.Add(z, new Model.EventDTO { StartDate = item });
                z++;
            }

            int i = 1;
            foreach (var item in model.EndDate)
            {

                events[i].EndDate = item;
                i++;
            }

            model.Events = events.Values.Select(x => new Model.EventDTO
            {
                UserID = user.UserID,
                TagID = model.TagID ?? null,
                StartDate = x.StartDate,
                EndDate = x.EndDate
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
                    ? (Status.Success, "Scheduler has been saved and your calendar has been updated")
                    : (Status.Failed, "There was as an issue with adding some or all of your scheduled events to your calendar");

                if (status.UpdateResponse == Status.Success)
                {
                    return RedirectToRoute(Url.Home(viewingId: (Guid?)null, combined: false, updateResponse: status.UpdateResponse, updateMsg: status.UpdateMsg));

                }
                else
                {
                    return RedirectToRoute(Url.Scheduler());
                }
            }
            
        }

        public async Task<ActionResult> Overview(DateFrequency? frequency = null, int? interval = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            await BaseViewModel(new MenuItem { Overview = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            string currentMonth = Utils.DateTime().ToString("MMMM");
            DateFrequency currentFrequency = Utils.ParseEnum<DateFrequency>(currentMonth);

            var dateFilter = new DateFilter
            {
                Frequency = frequency ?? DateFrequency.LastXDays,
                Interval = interval ?? 7,
                FromDateRange = fromDate ?? Utils.DateTime(),
                ToDateRange = toDate ?? Utils.DateTime()
            };

            var hoursWorkedInTag = await eventService.HoursSpentInTag(baseVM.User, dateFilter);

            return View("Overview",
                new OverviewVM
                {
                    Filter = dateFilter,
                    HoursWorkedInTag = hoursWorkedInTag
                });

        }
    }
}