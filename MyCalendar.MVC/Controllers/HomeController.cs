using MyCalendar.DTOs;
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
    public class HomeController : UserMvcController
    {
        private readonly IEventService eventService;

        public HomeController(IEventService eventService, IUserService userService, ITagService tagService) : base(userService, tagService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        public async Task<ActionResult> Index(Guid? viewingId = null, bool combined = false, Status? updateResponse = null, string updateMsg = null)
        {
            var user = await GetUser();

            if (user == null)
            {
                return View();
            }

            var viewModel = new CalendarVM
            {
                User = user,
                Users = await GetUsers(),
                UserTags = new TagsDTO { Tags = await GetUserTags() },
                UpdateStatus = (updateResponse, updateMsg),
                MenuItem = new MenuItem
                {
                    Home = viewingId == null && combined == false ? true : false,
                    Viewing = viewingId,
                    Combined = combined
                }
            };

            return View("Calendar", viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Index(int passcode)
        {
            var checkUser = await GetUser(passcode);

            if (checkUser != null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "The passcode was entered incorrectly";
            return View();
        }

        public async Task<JsonResult> GetEvents(Guid? viewingId = null, bool combined = false)
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
            var dto = events.Select(b => DTOs.EventDTO.MapFrom(b)).ToList();

            if (dto != null && dto.Any() && combined)
            {
                foreach (var e in dto)
                {
                    var initial = (await GetUserById(e.UserID)).Name.Substring(0, 1);
                    e.Description = $"({initial}) {e.Description}";
                    e.Subject = $"({initial}) {e.Subject}";
                }
            }

            var activeEvents = await eventService.GetCurrentActivityAsync();
            var currentActivity = await CurrentUserActivity(activeEvents);
 
            return new JsonResult { Data = new { events = dto, currentActivity }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public async Task<JsonResult> SaveEvent(EventVM e)
        {
            var status = false;

            if ((await GetUser()) == null)
            {
                return new JsonResult { Data = new { status, responseText = "You are not logged-in" } };
            }

            var dto = DTOs.EventDTO.MapFrom(e);
            dto.UserID = (await GetUser()).UserID;

            var daysBetweenDays = e.End.HasValue ? (e.End.Value.Date - e.Start.Date).Days : 0;

            if (string.IsNullOrEmpty(e.SplitDates) || daysBetweenDays == 0 || e.IsFullDay)
            {
                status = await eventService.SaveEvent(dto);
            }
            else
            {
                var events = new List<Model.EventDTO>();

                for (var date = e.Start; date <= e.End; date = date.AddDays(1))
                {
                    events.Add(new Model.EventDTO
                    {
                        StartDate = date,
                        EndDate = new DateTime(date.Year, date.Month, date.Day, e.End.Value.Hour, e.End.Value.Minute, 0),
                        Description = dto.Description,
                        EventID = dto.EventID,
                        IsFullDay = dto.IsFullDay,
                        TagID = dto.TagID,
                        Tentative = dto.Tentative,
                        UserID = dto.UserID
                    });
                }

                status = await eventService.SaveEvents(events);
            }

            return new JsonResult { Data = new { status } };
        }

        [HttpPost]
        public async Task<JsonResult> DeleteEvent(Guid eventID)
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

 
        public async Task<ActionResult> MultiAdd(int dates = 0, Guid? tagId = null, string times = null)
        {
            var user = await GetUser();

            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var viewModel = new SchedulerVM
            {
                Dates = dates,
                TagID = tagId,
                Times = times,
                User = user,
                Users = await GetUsers(),
                UserTags = new TagsDTO { Tags = await GetUserTags() },
                MenuItem = new MenuItem  { MultiAdd = true }
            };

            var scheduler = (SchedulerVM)TempData["scheduler"];

            if (scheduler != null)
            {
                viewModel.Dates = scheduler.Dates;
                viewModel.StartDate = scheduler.StartDate;
                viewModel.EndDate = scheduler.EndDate;
                viewModel.TagID = scheduler.TagID == Guid.Empty ? null : scheduler.TagID;
                viewModel.UpdateStatus = (scheduler.UpdateStatus.UpdateResponse, scheduler.UpdateStatus.UpdateMsg);
            }

            return View("MultiAdd", viewModel);
        }

        public ActionResult NewCalendar()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> MultiAdd(SchedulerVM model)
        {
            var user = await GetUser();

            (Status? UpdateResponse, string UpdateMsg) status = (null, null);

            if (user == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                var events = new Dictionary<int, Model.EventDTO>();
                var userId = (await GetUser()).UserID;

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
                    status = await eventService.SaveEvents(model.Events)
                        ? (Status.Success, "Scheduler has been saved and your calendar has been updated")
                        : (Status.Failed, "There was as an issue with adding some or all of your scheduled events to your calendar");

                    if (status.UpdateResponse == Status.Success)
                    {
                        return RedirectToAction("Index", new { viewingId = (Guid?)null, combined = false, updateResponse = status.UpdateResponse, updateMsg = status.UpdateMsg });

                    }
                    else
                    {
                        return RedirectToAction("MultiAdd");
                    }
                }
            }
        }

        public async Task<ActionResult> Settings(Status? updateResponse = null, string updateMsg = "")
        {
            var user = await GetUser();

            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var viewModel = new CalendarVM
            {
                User = user,
                Users = await GetUsers(),
                UserTags = new TagsDTO
                {
                    UserID = user.UserID,
                    Tags = await GetUserTags(),
                    Types = await eventService.GetTypes()

                },
                UpdateStatus = (UpdateResponse: updateResponse, UpdateMsg: updateMsg),
                MenuItem = new MenuItem { Settings = true }
            };

            return View("Settings", viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Settings(CalendarVM model)
        {
            (Status? UpdateResponse, string UpdateMsg) = await UpdateUser(model.User) == true 
                ? (Status.Success, "Your profile has been saved successfully")
                : (Status.Failed, "There was an issue with updating your profile");

            return RedirectToAction("Settings", new { updateResponse = UpdateResponse, updateMsg = UpdateMsg });
        }


        [HttpPost]
        public async Task<ActionResult> UpdateTags(TagsDTO tags)
        {
            (Status? UpdateResponse, string UpdateMsg) status = (null, null);

            tags.Id = tags.Id.Reverse().Skip(1).Reverse().ToArray();
            tags.Name = tags.Name.Reverse().Skip(1).Reverse().ToArray();
            tags.ThemeColor = tags.ThemeColor.Reverse().Skip(1).Reverse().ToArray();
            tags.TypeID = tags.TypeID.Reverse().Skip(1).Reverse().ToArray();
            tags.Privacy = tags.Privacy.Reverse().Skip(1).Reverse().ToArray();
            tags.UserCreatedId = tags.UserCreatedId.Reverse().Skip(1).Reverse().ToArray();

            var user = await GetUser();

            if ((await GetUser()) == null)
            {
                status = (Status.NotLoggedIn, null);
            }
            else
            {
                var tagsA = new Dictionary<int, Tag>();

                int z = 0;
                foreach (var item in tags.Id)
                {
                    tagsA.Add(z, new Tag { Id = item });
                    z++;
                }

                int i = 0;
                foreach (var item in tags.Name)
                {

                    tagsA[i].Name = item;
                    i++;
                }

                int a = 0;
                foreach (var item in tags.ThemeColor)
                {
                    tagsA[a].ThemeColor = item;
                    a++;
                }

                int t = 0;
                foreach (var item in tags.TypeID)
                {
                    tagsA[t].TypeID = item;
                    t++;
                }

                int b = 0;
                foreach (var item in tags.Privacy)
                {
                    tagsA[b].Privacy = item;
                    b++;
                }

                int c = 0;
                foreach (var item in tags.UserCreatedId)
                {
                    tagsA[c].UserID = item;
                    c++;
                }

                tags.Tags = tagsA.Values.Select(x => new Tag
                {
                    Id = x.Id,
                    UserID = x.UserID,
                    Name = x.Name,
                    ThemeColor = x.ThemeColor,
                    TypeID = x.TypeID,
                    Privacy = x.Privacy
                });

                status = await UpdateUserTags(tags.Tags, user.UserID)
                    ? (Status.Success, "Your tags has been updated successfully")
                    : (Status.Failed, "There was an issue updating your tags");
            }

            return RedirectToAction("Settings", new { updateResponse = status.UpdateResponse, updateMsg = status.UpdateMsg });
        }

        public async Task<ActionResult> Overview(DateFrequency? frequency =  null, int? interval = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var user = await GetUser();
            var users = await GetUsers();

            if (user == null)
            {
                return RedirectToAction("Index");
            }

            string currentMonth = Utils.DateTime().ToString("MMMM");

            var dateFilter = new DateFilter
            {
                Frequency = frequency ?? Utils.ParseEnum<DateFrequency>(currentMonth),
                Interval = interval ?? 1,
                FromDateRange = fromDate ?? Utils.DateTime(),
                ToDateRange = toDate ?? Utils.DateTime()
            };

            var events = (await eventService.GetAllAsync(userId: null, viewing: null, filter: dateFilter))
                .Where(x => x.UserID == user.UserID)
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
                            userName += ", " + string.Join(", ", users.Select(x => x.Name));
                            multiUser = true;
                        }

                        double minutesWorked = e.Sum(x => x.EndDate.HasValue ? Utils.MinutesBetweenDates(x.EndDate.Value, x.StartDate) : 1440);

                        if (minutesWorked > 0)
                        {
                            int hoursFromMinutes = Utils.GetHoursFromMinutes(minutesWorked);
                            int calculateHours = hoursFromMinutes >= 4 ? hoursFromMinutes / 4 : 0;
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
                new OverviewVM { 
                    User = user,
                    Users = await GetUsers(),
                    MenuItem = new MenuItem { Overview = true },
                    Filter = dateFilter, 
                    HoursWorkedInTag = hoursWorkedInTag 
            });

        }

        public ActionResult Logout()
        {
            LogoutUser();
            return RedirectToAction("Index");
        }

    }
}