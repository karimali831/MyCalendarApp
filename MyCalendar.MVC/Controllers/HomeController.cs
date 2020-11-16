
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyCalendar.Controllers
{
    public class HomeController : UserMvcController
    {
        private readonly ITypeService typeService;

        public HomeController(IUserService userService, IFeatureRoleService featureRoleService, ITypeService typeService) : base(userService, featureRoleService)
        {
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
        }

        public async Task<ActionResult> Index(Status? updateResponse = null, string updateMsg = null)
        {
            await BaseViewModel(new MenuItem { Home = true }, updateResponse, updateMsg);
            var baseVM = ViewData["BaseVM"] as BaseVM;

            var userCalendars = (await typeService.GetUserTypesAsync(baseVM.User.UserID)).Where(x => x.GroupId == TypeGroup.Calendars);
           
            foreach (var calendar in userCalendars)
            {
                calendar.InviteeName = (await GetUserById(calendar.UserCreatedId)).Name;
            }

            return View(new CalendarVM { UserCalendars = userCalendars });
        }

        public async Task<ActionResult> ChangeLog()
        {
            await BaseViewModel(new MenuItem { None = true });
            var changes = Helpers.ChangeLog.GetChangeList();
            return View(new ChangeLogVM { ChangeLog = changes });
        }
    }
}