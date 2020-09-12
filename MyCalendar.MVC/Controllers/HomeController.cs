using MyCalendar.DTOs;
using MyCalendar.Model;
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
    public class HomeController : UserMvcController
    {
        private readonly IEventService eventService;

        public HomeController(IEventService eventService, IUserService userService, ITagService tagService) : base(userService, tagService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        public async Task<ActionResult> Index(Guid? viewingId = null, bool combined = false)
        {
            var user = await GetUser();

            if (user != null)
            {
                return View("Calendar", 
                    new CalendarVM 
                    { 
                        User = user,
                        Users = await GetUsers(),
                        UserTags = new TagsDTO { Tags = await GetUserTags() },
                        Viewing = viewingId,
                        Combined = combined
                    });
            }
            
            return View();
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
            if ((await GetUser()) == null)
            {
                return new JsonResult { Data = new { status = false, responseText = "You are not logged-in" } };
            }

            var dto = DTOs.EventDTO.MapFrom(e);
            dto.UserID = (await GetUser()).UserID;

            var status = await eventService.SaveEvent(dto);
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

        public async Task<ActionResult> Settings(int? status = null)
        {
            var user = await GetUser();

            var viewModel = new CalendarVM
            {
                Settings = true,
                SettingsUpdated = status,
                User = user,
                Users = await GetUsers(),
                UserTags = new TagsDTO
                {
                    UserID = user.UserID,
                    Tags = await GetUserTags(),
                    Types = await eventService.GetTypes()

                }
            };

            return View("Settings", viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Settings(CalendarVM model)
        {
            var status = await UpdateUser(model.User) == true ? 1 : 0;
            return RedirectToAction("Settings", new { status });
        }


        [HttpPost]
        public async Task<ActionResult> UpdateTags(TagsDTO tags)
        {

            var tagsA = new Dictionary<int, Tag>();
            var userId = (await GetUser()).UserID;

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

            var status = await UpdateUserTags(tags.Tags, userId) == true ? 1 : 0;
            return RedirectToAction("Settings", new { status });
        }

        public ActionResult Logout()
        {
            LogoutUser();
            return RedirectToAction("Index");
        }

    }
}