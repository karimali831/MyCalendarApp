
using DFM.ExceptionHandling;
using DFM.ExceptionHandling.Sentry;
using MyCalendar.DTOs;
using MyCalendar.Model;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyCalendar.Controllers
{
    public class UserMvcController : Controller
    {
        private readonly IExceptionHandlerService exceptionHandlerService;
        private readonly IUserService userService;
        private readonly ITagService tagService;
        private readonly string AuthenticationName = "iCalendarApp-Authentication";

        public UserMvcController(IUserService userService, ITagService tagService)
        {
            this.exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        }

        public async Task<IList<User>> GetUsers()
        {
            var users = await userService.GetAllAsync();
            var user = await GetUser();

            if (user != null)
            {
                users = users.Where(x => x.UserID != user.UserID);
            }

            return users.ToList();
        }

        public async Task<IEnumerable<Tag>> GetUserTags()
        {
            var user = await GetUser();

            if (user == null)
            {
                return Enumerable.Empty<Tag>();
            }

            var userTags = await tagService.GetUserTagsAsync(user.UserID);
            
            if (userTags != null)
            {
                return userTags;
            }

            return Enumerable.Empty<Tag>();
        }


        public async Task<User> GetUser(int? passcode = null)
        {
            if (passcode.HasValue)
            {
                var user = await userService.GetAsync(passcode.Value);

                if (user != null)
                {
                    Session[AuthenticationName] = user.Passcode;
                    return user;
                }
                else
                {
                    RedirectToAction("Index");
                }
            }
            else
            {
                if (Session[AuthenticationName] != null)
                {
                    var user = await userService.GetAsync(int.Parse(Session[AuthenticationName].ToString()));

                    if (user != null)
                    {
                        user.Authenticated = true;
                        return user;
                    }
                    else
                    {
                        RedirectToAction("Index");
                    }
                }
                else
                {
                    RedirectToAction("Index");
                }
            }

            return null;
        }

        public void LogoutUser()
        {
            Session.Remove(AuthenticationName);
        }

        public async Task<bool> UpdateUser(User user)
        {
            return await userService.UpdateAsync(user);
        }

        public async Task<bool> UpdateUserTags(IEnumerable<Tag> tags, Guid userID)
        {
            return await tagService.UpdateUserTagsAsync(tags, userID);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            exceptionHandlerService.ReportException(filterContext.Exception).Submit();

            filterContext.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml",
                ViewData = new ViewDataDictionary(filterContext.Controller.ViewData)
                {
                    Model = new ErrorVM { Exception = filterContext.Exception }
                }
            };
        }
    }
}