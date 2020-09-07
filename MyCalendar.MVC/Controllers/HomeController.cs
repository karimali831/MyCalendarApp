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

        public HomeController(IEventService eventService, IUserService userService) : base(userService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        public async Task<ActionResult>  Index()
        {
            var user = await GetUser();

            if (user != null)
            {
                return View("Calendar", new CalendarVM { User = user });
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
            dto.UserID = (await GetUser()).UserID;

            var status = await eventService.SaveEvent(dto);
            return new JsonResult { Data = new { status } };
        }

        [HttpPost]
        public async Task<JsonResult> DeleteEvent(Guid eventID)
        {
            // check the event was created by the user
            var e = await eventService.GetAsync(eventID);
            var userId = (await GetUser()).UserID;

            var status = e.UserID != userId ? false : await eventService.DeleteEvent(eventID);
            return new JsonResult { Data = new { status } };
        }
    }
}