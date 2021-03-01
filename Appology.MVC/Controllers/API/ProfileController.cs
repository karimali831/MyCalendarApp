using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Appology.DTOs;
using Appology.Enums;
using Appology.Helpers;
using Appology.MiCalendar.Helpers;
using Appology.MiCalendar.Model;
using Appology.MiCalendar.Service;
using Appology.Model;
using Appology.Service;

namespace Appology.Controllers.Api
{
    [RoutePrefix("api/profile")]
    [CamelCaseControllerConfig]
    public class ProfileController : ApiController
    {
        private readonly IUserService userService;
        private readonly ITypeService typeService;
        private readonly IDocumentService documentService;
        private readonly IEventService eventService;
        private readonly IFeatureRoleService featureRoleService;
        private readonly string rootUrl = ConfigurationManager.AppSettings["RootUrl"];

        public ProfileController(
            IUserService userService, 
            ITypeService typeService, 
            IEventService eventService,
            IDocumentService documentService,
            IFeatureRoleService featureRoleService)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            this.featureRoleService = featureRoleService ?? throw new ArgumentNullException(nameof(featureRoleService));
        }

        private async Task<User> GetUser()
        {
            bool isLocal = this.rootUrl == "http://localhost:53822";
            return await userService.GetUser(isLocal ? "karimali831@googlemail.com" : null);
        }

        [Route("usertypes/{typeGroup}")]
        [HttpGet]
        public async Task<HttpResponseMessage> UserTypes(TypeGroup typeGroup)
        {
            var user = await GetUser();
            var userTypes = await typeService.GetAllByUserIdAsync(user.UserID, typeGroup);

            return Request.CreateResponse(HttpStatusCode.OK, userService.GetUserTypes(user, userTypes));
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

            var url = new System.Web.Mvc.UrlHelper(HttpContext.Current.Request.RequestContext);

            return Request.CreateResponse(HttpStatusCode.OK, new { 
                status = user != null,
                groups = groups.Select(x => new
                {
                    x.Id,
                    x.InviteDescription,
                    x.Nodes
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
                    userCalendars = userService.GetUserTypes(user, userCalendars),
                    userTypes = userService.GetUserTypes(user, userTypes),
                    inviterShareLink = url.InviterShareLink(user.UserID),
                    userBuddys = userBuddys.Select(x => new
                    {
                        x.UserID,
                        x.Name,
                        avatar = CalendarUtils.AvatarSrc(x.UserID, x.Avatar, x.Name)
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

        [Route("removeuserbuddy/{buddyId}/{existConfirm}")]
        [HttpGet]
        public async Task<HttpResponseMessage> RemoveUserBuddy(Guid buddyId, bool existConfirm)
        {
            (Variant Variant, string Message) response = (Variant.Light, "");
            var user = await GetUser();
            var removee = await userService.GetByUserIDAsync(buddyId);

            var removeeBuddys = await userService.GetBuddys(removee.UserID);
            var userBuddys = await userService.GetBuddys(user.UserID);

            var userTypes = (await typeService.GetAllUserTypesAsync(user.UserID)).Where(x => x.InviteeIdsList.Contains(buddyId));
            var removeeTypes = (await typeService.GetAllUserTypesAsync(removee.UserID)).Where(x => x.InviteeIdsList.Contains(user.UserID));

            if (!existConfirm)
            {
                if (userTypes.Any() || removeeTypes.Any())
                {
                    response = (Variant.Warning, $"You will no longer be able to participate in the current sharing activities with {removee.Name}. Remove this buddy anyway?");
                }
                else
                {
                    response = (Variant.Primary, $"Are you sure you want to remove this buddy?");
                }

            }
            else if ((!userTypes.Any() && !removeeTypes.Any()) || existConfirm)
            {
                var updUserBuddys = string.Join(",", userBuddys
                    .Where(x => x.UserID != buddyId)
                    .Select(x => x.UserID)) ?? null;

                var updRemoveBuddys = string.Join(",", removeeBuddys
                    .Where(x => x.UserID != user.UserID)
                    .Select(x => x.UserID)) ?? null;


                var updateUserTypes = string.Join(",", userTypes
                    .Where(x => x.UserCreatedId != buddyId)
                    .Select(x => x.UserCreatedId)) ?? null;


                var updateRemoveeTypes = string.Join(",", removeeTypes
                    .Where(x => x.UserCreatedId != user.UserID)
                    .Select(x => x.UserCreatedId)) ?? null;

                var updateUserRemovee = await userService.UpdateBuddys(updRemoveBuddys, buddyId);
                var updateUserRemover = await userService.UpdateBuddys(updUserBuddys, user.UserID);
                var updateTypesRemovee = await typeService.UpdateInvitees(updateRemoveeTypes, buddyId);
                var updateTypesRemover = await typeService.UpdateInvitees(updateUserTypes, user.UserID);

                response = (Variant.Success, $"You and {removee.Name} are no longer buddys");
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                responseVariant = response.Variant,
                responseMsg = response.Message
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

        [Route("deleteusertype/{Id}/{groupId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> DeleteUserType(int Id, TypeGroup groupId)
        {
            (bool status, string Message) response = (true, "");
            var user = await GetUser();

            var type = await typeService.GetAsync(Id);

            if (user.UserID != type.UserCreatedId)
            {
                response = (false, "An error occured");
            }
            else if (type.GroupId == TypeGroup.Calendars && await eventService.EventExistsInCalendar(Id))
            {
                response = (false, "Events exist in this calendar");
            }
            else if (type.GroupId == TypeGroup.TagGroups && await userService.GroupExistsInTag(Id))
            {
                response = (false, "Event tags exist in this tag group");
            }
            else if (type.GroupId == TypeGroup.DocumentFolders)
            {
  
                if (await documentService.DocumentsExistsInGroup(Id))
                {
                    response = (false, "Documents exist in this folder");
                }
                else
                {
                    var exists = new List<bool>() { false };
                    var subTypeIds = await typeService.GetAllIdsByParentTypeIdAsync(Id);

                    if (subTypeIds != null && subTypeIds.Any())
                    {
                        foreach (var subTypeId in subTypeIds)
                        {
                            exists.Add(await documentService.DocumentsExistsInGroup(subTypeId));
                        }

                    }

                    if (exists.All(x => !x))
                    {
                        if (subTypeIds != null && subTypeIds.Any())
                        {
                            foreach (var subTypeId in subTypeIds)
                            {
                                await userService.DeleteUserType(subTypeId, user.UserID);
                            }
                        }

                        response = await userService.DeleteUserType(Id, user.UserID);
                    }
                    else
                    {
                        response = (false, "Documents exist in sub-folders");
                    }
                }
            }
            else
            { 
                response = await userService.DeleteUserType(Id, user.UserID);
            }
            

            var userTypes = await typeService.GetAllByUserIdAsync(user.UserID, groupId);
    
            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                response.status,
                responseMsg = response.Message,
                userTypes = userService.GetUserTypes(user, userTypes)
            });
        }

        [Route("saveusertype")]
        [HttpPost]
        public async Task<HttpResponseMessage> SaveUserType(UserTypeDTO dto)
        {
            var user = await GetUser();
            dto.UserCreatedId= user.UserID;

            var response = await userService.SaveUserType(dto);
            var userTypes = await typeService.GetAllByUserIdAsync(user.UserID, dto.GroupId);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                status = response.Status,
                responseMsg = response.Status ? "Successfully saved" : "An error occured",
                userTypes = userService.GetUserTypes(user, userTypes)
            });
        }

        [Route("moveusertype/{id}/{groupId}/{superTypeId?}")]
        [HttpGet]
        public async Task<HttpResponseMessage> MoveUserType(int id, TypeGroup groupId, int? superTypeId)
        {
            var user = await GetUser();
            var status = await typeService.MoveTypeAsync(id, superTypeId);
            var userTypes = await typeService.GetAllByUserIdAsync(user.UserID, groupId);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                status,
                responseMsg = status ? 
                    "Folder successfully moved" : 
                    "There was an issue with moving the folder",
                userTypes = userService.GetUserTypes(user, userTypes)
            });
        }
    }
}
