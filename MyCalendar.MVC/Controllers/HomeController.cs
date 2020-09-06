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
            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }
    }
}