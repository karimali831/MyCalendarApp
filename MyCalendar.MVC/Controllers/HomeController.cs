using MyCalendar.DTOs;
using MyCalendar.Enum;
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

            if (combined)
            {
                viewing = null;
            }
            else
            {
               viewing = viewingId != null ? viewingId : (await GetUser()).UserID;
            }

            var events = await eventService.GetAllAsync(viewing);
            var dto = events.Select(b => DTOs.EventDTO.MapFrom(b)).ToList();

            return new JsonResult { Data = dto, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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

            if (string.IsNullOrEmpty(e.SplitDates))
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

 
        public async Task<ActionResult> MultiAdd(int dates = 0, Guid? tagId = null)
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

            var user = await GetUser();

            if ((await GetUser()) == null)
            {
                status = (Status.NotLoggedIn, null);
            }
            else
            {
                var tagsA = new Dictionary<int, Tag>();

                int z = 1;
                foreach (var item in tags.Id)
                {
                    tagsA.Add(z, new Tag { Id = item });
                    z++;
                }

                int i = 1;
                foreach (var item in tags.Name)
                {

                    tagsA[i].Name = item;
                    i++;
                }

                int a = 1;
                foreach (var item in tags.ThemeColor)
                {
                    tagsA[a].ThemeColor = item;
                    a++;
                }

                int t = 1;
                foreach (var item in tags.TypeID)
                {
                    tagsA[t].TypeID = item;
                    t++;
                }

                tags.Tags = tagsA.Values.Select(x => new Tag
                {
                    Id = x.Id,
                    UserID = tags.UserID,
                    Name = x.Name,
                    ThemeColor = x.ThemeColor,
                    TypeID = x.TypeID
                })
                .Where(x => !string.IsNullOrEmpty(x.Name));

                status = await UpdateUserTags(tags.Tags, user.UserID)
                    ? (Status.Success, "Your tags has been updated successfully")
                    : (Status.Failed, "There was an issue updating your tags");
            }

            return RedirectToAction("Settings", new { updateResponse = status.UpdateResponse, updateMsg = status.UpdateMsg });
        }

        public ActionResult Logout()
        {
            LogoutUser();
            return RedirectToAction("Index");
        }

    }
}