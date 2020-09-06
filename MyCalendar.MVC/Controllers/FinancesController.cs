using Ninject;
using MyCalendar.DTOs;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Controllers
{
    public class CalendarController : Controller
    {
        private readonly ICalendarService CalendarService;

        public CalendarController(ICalendarService CalendarService)
        {
            this.CalendarService = CalendarService ?? throw new ArgumentNullException(nameof(CalendarService));
        }

        public async Task<ActionResult> Index()
        {
            var Calendar = await CalendarService.GetAllAsync();
            return View(Calendar);
        }
    }
}