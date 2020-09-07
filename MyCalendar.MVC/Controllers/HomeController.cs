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
        private readonly IUserService userService;
        private readonly static string AuthenticationName = "iCalendarApp-Authentication";

        public HomeController(IEventService eventService, IUserService userService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<ActionResult>  Index()
        {
            var users = await userService.GetAllAsync();

            if (Session[AuthenticationName] != null)
            {
                if (users.Select(x => x.Passcode.ToString()).Contains(Session[AuthenticationName].ToString()))
                {
                    return RedirectToAction("Calendar");
                }
            }

            return View();
        }

        public async Task<ActionResult> Calendar()
        {
            var user = await userService.GetAsync(int.Parse(Session[AuthenticationName].ToString()));

            if (user != null)
            {
                return View("Calendar",
                    new CalendarVM
                    {
                        Name = user.Name,
                        Authenticated = true
                    });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Index(int passcode)
        {
            var checkUser = await userService.GetAsync(passcode);

            if (checkUser != null)
            {
                Session[AuthenticationName] = checkUser.Passcode;
                return RedirectToAction("Calendar");
            }

            ViewBag.ErrorMessage = "The passcode was entered incorrectly";
            return View();
        }

        public async Task<JsonResult> GetEvents()
        {
            var events = await eventService.GetAllAsync();
            var dto = events.Select(b => EventDTO.MapFrom(b)).ToList();
            return new JsonResult { Data = dto, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public async Task<JsonResult> SaveEvent(EventDTO e)
        {
            var dto = EventDTO.MapTo(e);
            var status = await eventService.SaveEvent(dto);
            return new JsonResult { Data = new { status } };
        }

        [HttpPost]
        public async Task<JsonResult> DeleteEvent(Guid eventID)
        {
            var status = await eventService.DeleteEvent(eventID);
            return new JsonResult { Data = new { status } };
        }
    }
}