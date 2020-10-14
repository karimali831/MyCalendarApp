using MyCalendar.DTOs;
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Controllers
{
    public class EventController : UserMvcController
    {
        private readonly IEventService eventService;

        public EventController(
            IEventService eventService, ICronofyService cronofyService, IUserService userService, ITagService tagService) :
            base(userService, cronofyService, tagService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
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
                viewing = viewingId != null ? viewingId : (await GetUser()).UserID;
            }


            var events = await eventService.GetAllAsync(user.UserID, viewing);
            var dto = events.Select(b => EventDTO.MapFrom(b)).ToList();

            var activeEvents = await eventService.GetCurrentActivityAsync();
            var currentActivity = await CurrentUserActivity(activeEvents);

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

            e.UserID = (await GetUser()).UserID;

            status = await eventService.SaveEvent(e);
            return new JsonResult { Data = new { status } };
        }

        [HttpPost]
        public async Task<JsonResult> Delete(Guid eventID)
        {
            if ((await GetUser()) == null)
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
                return RedirectToAction("MultiAdd");
            }
            else
            {
                //if (model.Icloud)
                //{
                //    try
                //    {
                //        foreach (var e in model.Events)
                //        {
                //            cronofyService.UpsertEvent(e.EventID, newEvent.CalendarId, newEvent.Summary, newEvent.Description, newEvent.Start, newEvent.End, new Location(newEvent.LocationDescription, newEvent.Latitude, newEvent.Longitude));
                //        }


                //    }
                //    catch (CronofyResponseException ex)
                //    {
                //        newEvent.SetError(ex);
                //    }
                //}

                status = await eventService.SaveEvents(model.Events)
                    ? (Status.Success, "Scheduler has been saved and your calendar has been updated")
                    : (Status.Failed, "There was as an issue with adding some or all of your scheduled events to your calendar");

                if (status.UpdateResponse == Status.Success)
                {
                    return RedirectToAction("Index", "Home", new { viewingId = (Guid?)null, combined = false, updateResponse = status.UpdateResponse, updateMsg = status.UpdateMsg });

                }
                else
                {
                    return RedirectToAction("MultiAdd");
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

            var events = (await eventService.GetAllAsync(userId: null, viewing: null, filter: dateFilter))
                .Where(x => x.UserID == baseVM.User.UserID || x.Privacy == TagPrivacy.Shared)
                .GroupBy(x => x.TagID);

            var hoursWorkedInTag = new List<HoursWorkedInTag>();

            if (events != null && events.Any())
            {
                foreach (var e in events)
                {
                    if (e.Key != Guid.Empty)
                    {
                        var tag = await GetTag(e.Key);
                        var type = await eventService.GetTypeAsync(tag.TypeID);
                        string userName = "You";
                        bool multiUser = false;

                        if (tag.Privacy == TagPrivacy.Shared)
                        {
                            userName += ", " + string.Join(", ", baseVM.Users.Select(x => x.Name));
                            multiUser = true;
                        }

                        double minutesWorked = e.Sum(x => x.EndDate.HasValue ? Utils.MinutesBetweenDates(x.EndDate.Value, x.StartDate) : 1440);

                        if (minutesWorked > 0)
                        {
                            int hoursFromMinutes = Utils.GetHoursFromMinutes(minutesWorked);
                            int calculateHours = hoursFromMinutes >= 672 ? hoursFromMinutes / 4 : 0;
                            string averaged = calculateHours >= 1 ? $" averaging {calculateHours} hour{(calculateHours > 1 ? "s" : "")} a week" : "";
                            string text;

                            if (dateFilter.Frequency == DateFrequency.Upcoming)
                            {
                                string multipleEvents = e.Count() > 1 ? "have upcoming events totalling" : "have an upcoming event for";
                                text = string.Format($"{userName} {multipleEvents} {Utils.HoursDurationFromMinutes(minutesWorked)} with {tag.Name}");
                            }
                            else
                            {
                                text = string.Format($"{userName} spent {Utils.HoursDurationFromMinutes(minutesWorked)} {averaged} with {tag.Name}");
                            }

                            if (tag.Name == "Flex")
                            {
                                text += $" earning approx £{hoursFromMinutes * 13}";
                            }

                            hoursWorkedInTag.Add(new HoursWorkedInTag
                            {
                                Text = text,
                                MultiUsers = multiUser,
                                Color = tag.ThemeColor,
                                TypeName = type.Name
                            });
                        }
                    }
                }
            }

            return View("Overview",
                new OverviewVM
                {
                    User = baseVM.User,
                    Users = await GetUsers(),
                    MenuItem = new MenuItem { Overview = true },
                    Filter = dateFilter,
                    HoursWorkedInTag = hoursWorkedInTag
                });

        }
    }
}