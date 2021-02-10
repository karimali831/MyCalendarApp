using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Appology.DTOs;
using Appology.Enums;
using Appology.MiCalendar.Helpers;
using Appology.MiCalendar.Model;
using Appology.MiCalendar.Service;
using Appology.Model;
using Appology.Service;

namespace Appology.Controllers.Api
{
    [RoutePrefix("api/profile")]
    [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
    [CamelCaseControllerConfig]
    public class ProfileController : ApiController
    {
        private readonly IUserService userService;
        private readonly ITypeService typeService;
        private readonly IEventService eventService;
        private readonly IFeatureRoleService featureRoleService;
        private readonly string rootUrl = ConfigurationManager.AppSettings["RootUrl"];

        public ProfileController(
            IUserService userService, 
            ITypeService typeService, 
            IEventService eventService,
            IFeatureRoleService featureRoleService)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.featureRoleService = featureRoleService ?? throw new ArgumentNullException(nameof(featureRoleService));
        }

        private async Task<User> GetUser()
        {
            bool isLocal = this.rootUrl == "http://localhost:53822";
            return await userService.GetUser(isLocal ? "karimali831@googlemail.com" : null);
        }

        [Route("user")]
        [HttpGet]
        public async Task<HttpResponseMessage> LoadUser()
        {
            var user = await GetUser();
            var userBuddys = await userService.GetBuddys(user.UserID);
            var userTags = await userService.GetUserTags(user.UserID);
            var userCalendars = await userService.UserCalendars(user.UserID);
            var userTypes = await typeService.GetAllByUserIdAsync(user.UserID);
            var groups = await featureRoleService.GetGroupsAsync();

            object types(IEnumerable<Types> t) => t.Select(x => new
            {
                x.Id,
                x.UserCreatedId,
                x.Name,
                invitee = user.UserID != x.UserCreatedId ? x.InviteeName : null,
                x.InviteeIdsList,
                selected = user.SelectedCalendarsList.Contains(x.Id),
                x.GroupId
            });


            return Request.CreateResponse(HttpStatusCode.OK, new { 
                status = user != null,
                groups = groups.Select(x => new
                {
                    x.Id,
                    x.InviteDescription
                }),
                user = new 
                {
                    userInfo = new
                    {
                        user.UserID,
                        user.Name,
                        user.PhoneNumber,
                        user.Email
                    },
                    avatar = CalendarUtils.AvatarSrc(user.UserID, user.Avatar, user.Name),
                    userCalendars = types(userCalendars),
                    userTypes = types(userTypes),
                    userBuddys = userBuddys.Select(x => new
                    {
                        x.UserID,
                        x.Name
                    }),
                    userTags = userTags.Select(x => new
                    {
                        x.Id,
                        x.TypeID,
                        x.Name,
                        x.ThemeColor
                    }),
                    calendarSettings = new
                    {
                        user.EnableCronofy,
                        user.DefaultCalendarView,
                        user.DefaultNativeCalendarView,
                        user.SelectedCalendars
                    }
                }
            });
        }

        [Route("saveuserinfo")]
        [HttpPost]
        public async Task<HttpResponseMessage> SaveUserInfo(UserInfoDTO dto)
        {
            var user = await GetUser();

            dto.UserID = user.UserID;
            dto.Password = string.IsNullOrEmpty(dto.Password) ? user.Password : dto.Password;

            var status = await userService.SaveUserInfo(dto);
            return Request.CreateResponse(HttpStatusCode.OK, status);
        }

        [Route("savecalendarsettings")]
        [HttpPost]
        public async Task<HttpResponseMessage> SaveCalendarSettings(CalendarSettingsDTO dto)
        {
            var user = await GetUser();
            dto.UserId = user.UserID;
    
            var status = await userService.SaveCalendarSettings(dto);
            return Request.CreateResponse(HttpStatusCode.OK, status);
        }

        [Route("saveusertags")]
        [HttpPost]
        public async Task<HttpResponseMessage> SaveUserTags(Tag[] dto)
        {
            var user = await GetUser();
            var status = await userService.UpdateUserTagsAsync(dto.ToList(), user.UserID);
            return Request.CreateResponse(HttpStatusCode.OK, status);
        }

        [Route("deleteusertype/{Id}")]
        [HttpGet]
        public async Task<HttpResponseMessage> DeleteUserType(int Id)
        {
            (bool status, string Message) response = (false, "");
            var user = await GetUser();

            var type = await typeService.GetAsync(Id);

            if (user.UserID != type.UserCreatedId)
            {
                response = (false, "An error occured");
            }
            else if (type.GroupId == TypeGroup.Calendars && await eventService.EventExistsInCalendar(Id))
            {
                response = (false, "Cannot remove because events exist in this Calendar - remove associated events or change the tag in these events");
            }
            else if (type.GroupId == TypeGroup.TagGroups && await userService.GroupExistsInTag(Id))
            {
                response = (false, "Cannot remove because this group exists in tags - remove associatd tags or change the group in these tags");
            }
            else
            {
                response = await userService.DeleteUserType(Id, user.UserID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                response.status,
                response.Message
            });
        }

        [Route("saveusertype")]
        [HttpPost]
        public async Task<HttpResponseMessage> SaveUserType(UserTypeDTO dto)
        {
            var user = await GetUser();
            dto.UserCreatedId= user.UserID;

            var response = await userService.SaveUserType(dto);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                type = !response.Status ? null : new 
                {
                    response.UserType.Id,
                    response.UserType.Name,
                    response.UserType.UserCreatedId,
                    invitee = user.UserID != response.UserType.UserCreatedId ? response.UserType.InviteeName : null,
                    response.UserType.InviteeIdsList,
                    selected = user.SelectedCalendarsList.Contains(response.UserType.Id),
                    response.UserType.GroupId
                }
            });
        }
    }
}
