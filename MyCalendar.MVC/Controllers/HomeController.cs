using Cronofy;
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
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Controllers
{
    public class HomeController : UserMvcController
    {
        private readonly IEventService eventService;
        private readonly ICronofyService cronofyService;

        public HomeController(
            IEventService eventService, IUserService userService, ICronofyService cronofyService) : base(userService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.cronofyService = cronofyService ?? throw new ArgumentNullException(nameof(cronofyService));
        }

        public ActionResult Login()
        {
            ViewBag.ErrorMessage = TempData["ErrorMsg"];
            return View();
        }

        public async Task<ActionResult> Index(Guid? viewingId = null, bool combined = false, Status? updateResponse = null, string updateMsg = null)
        {
            var menuItem = new MenuItem
            {
                Home = viewingId == null && combined == false ? true : false,
                Viewing = viewingId,
                Combined = combined
            };

            await BaseViewModel(menuItem, updateResponse, updateMsg);
            return View("Calendar");
        }

        [HttpPost]
        public async Task<ActionResult> Index(int passcode)
        {
            var user = await GetUser(passcode);

            if (user == null)
            {
                TempData["ErrorMsg"] = "The passcode was entered incorrectly";
                return RedirectToAction("Login");
            }
            else
            {
                Response.SetCookie(new HttpCookie(AuthenticationName, passcode.ToString()));
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> ChangeLog()
        {
            await BaseViewModel(new MenuItem { None = true });
            var changes = Helpers.ChangeLog.GetChangeList();
            return View(new ChangeLogVM { ChangeLog = changes });
        }

 
        public async Task<ActionResult> Settings(Status? updateResponse = null, string updateMsg = "")
        {
            var menuItem = new MenuItem { Settings = true };
            await BaseViewModel(menuItem, updateResponse, updateMsg);
            var baseVM = ViewData["BaseVM"] as BaseVM;

            var viewModel = new SettingsVM
            {
                User = baseVM.User,
                Types = await eventService.GetTypes(),
                CronofyCalendarAuthUrl = cronofyService.GetAuthUrl()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Settings(SettingsVM model)
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

            tags.Tags = new List<Tag>(tagsA.Values);

            status = await UpdateUserTags(tags.Tags, user.UserID)
                ? (Status.Success, "Your tags has been updated successfully")
                : (Status.Failed, "There was an issue updating your tags");
            

            return RedirectToAction("Settings", new { updateResponse = status.UpdateResponse, updateMsg = status.UpdateMsg });
        }

        public ActionResult Logout()
        {
            LogoutUser();
            return RedirectToAction("Login");
        }

    }
}