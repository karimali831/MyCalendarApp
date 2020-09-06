using MyCalendar.DTOs;
using MyCalendar.Model;
using MyCalendar.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEventService eventService;

        public HomeController(IEventService eventService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        public ActionResult Index()
        {
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