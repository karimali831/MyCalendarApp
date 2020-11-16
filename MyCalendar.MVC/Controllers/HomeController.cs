
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
        private readonly ITypeService typeService;
        private readonly IDocumentService documentService;

        public HomeController(IUserService userService, IFeatureRoleService featureRoleService, ITypeService typeService, IDocumentService documentService) : base(userService, featureRoleService)
        {
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
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
            var changeLogDocs = await documentService.GetAllByTypeIdAsync((int)TypeIdentifier.ChangeLog);
            return View(changeLogDocs);
        }
    }
}