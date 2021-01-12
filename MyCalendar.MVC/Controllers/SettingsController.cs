using MyCalendar.Controllers;
using MyCalendar.DTOs;
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Website.Controllers
{
    public class SettingsController : UserMvcController
    {
        private readonly ITypeService typeService;
        private readonly ICronofyService cronofyService;
        private readonly IEventService eventService;

        public SettingsController(
            IUserService userService, 
            IFeatureRoleService featureRoleService, 
            ITypeService typeService,
            INotificationService notificationService,
            IEventService eventService,
            ICronofyService cronofyService) : base(userService, featureRoleService, notificationService)
        {
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
            this.cronofyService = cronofyService ?? throw new ArgumentNullException(nameof(cronofyService));
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));

        }

        public async Task<ActionResult> Index(Status? updateResponse = null, string updateMsg = "")
        {
            var menuItem = new MenuItem { Settings = true };
            await BaseViewModel(menuItem, updateResponse, updateMsg);

            var baseVM = ViewData["BaseVM"] as BaseVM;
            var avatars = Directory.EnumerateFiles(Server.MapPath("~/Content/img/avatar/pk1"));

            var viewModel = new SettingsVM
            {
                User = baseVM.User,
                UserTags = await UserTags(baseVM.User.UserID),
                Buddys = baseVM.Buddys,
                UserTypes = await typeService.GetAllByUserIdAsync(baseVM.User.UserID),
                AccessibleGroups = baseVM.AccessibleGroups,
                Avatars = avatars,
                CronofyCalendarAuthUrl = cronofyService.GetAuthUrl()
            };

            return View(viewModel);
        }


        [ValidateAntiForgeryToken()]
        [HttpPost]
        public async Task<ActionResult> Index(SettingsVM model)
        {
            (Status? UpdateResponse, string UpdateMsg) = await UpdateUser(model.User) == true
                ? (Status.Success, "Your profile has been saved successfully")
                : (Status.Failed, "There was an issue with updating your profile");

            return RedirectToRoute(Url.Settings(UpdateResponse, UpdateMsg));
        }

        [ValidateAntiForgeryToken()]
        [HttpPost]
        public async Task<ActionResult> UpdateType(TypeVM dto)
        {
            (Status? UpdateResponse, string UpdateMsg) status = (null, null);

            var inviteeIdsList = new List<Guid>();

            if (dto.InviteeId != null)
            {
                foreach (var it in dto.InviteeId)
                {
                    if (Guid.TryParse(it, out Guid inviteeId) && inviteeId != Guid.Empty)
                    { 
                        inviteeIdsList.Add(inviteeId);
                    }
                }
            }

            var update = new Types
            {
                Id = dto.Id,
                Name = dto.Name,
                InviteeIds = (inviteeIdsList.Any() ? string.Join(",", inviteeIdsList) : null),
                SuperTypeId = dto.SuperTypeId,
                UserCreatedId = dto.UserCreatedId,
                GroupId = dto.GroupId
            };

            status = await typeService.UpdateTypeAsync(update)
                ? (Status.Success, "Successfully updated")
                : (Status.Failed, "An error occured");

            return RedirectToRoute(Url.Settings(status.UpdateResponse, status.UpdateMsg));
        }

        public async Task<JsonResult> MoveType(string Id, int? moveToId = null)
        {
            bool status = true;
            string responseText = "";
            if (int.TryParse(Id, out int typeId))
            {
                var menuItem = new MenuItem { Settings = true };
                await BaseViewModel(menuItem);

                var baseVM = ViewData["BaseVM"] as BaseVM;
                var type = await typeService.GetAsync(typeId);

                if (baseVM.User.UserID != type.UserCreatedId)
                {
                    status = false;
                    responseText = "The folder you're attempting to move was not created by you";
                }

                if (moveToId.HasValue)
                {
                    var superType = await typeService.GetAsync(moveToId.Value);

                    if (baseVM.User.UserID != superType.UserCreatedId)
                    {
                        status = false;
                        responseText = "The folder you're attempting to move was not created by you";
                    }
                }

                if (status == true)
                {
                    status = await typeService.MoveTypeAsync(typeId, moveToId);
                }
            }

            return new JsonResult { Data = new { status, responseText }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public async Task<ActionResult> RemoveType(int Id)
        {
            (Status? UpdateResponse, string UpdateMsg) status = (null, null);

            var menuItem = new MenuItem { Settings = true };
            await BaseViewModel(menuItem);

            var baseVM = ViewData["BaseVM"] as BaseVM;
            var type = await typeService.GetAsync(Id);

            if (baseVM.User.UserID != type.UserCreatedId)
            {
                status.UpdateResponse = Status.Failed;
                status.UpdateMsg = "Cannot remove - type not created by you";
            }
            else if (type.GroupId == TypeGroup.Calendars && await eventService.EventExistsInCalendar(Id))
            {
                status.UpdateResponse = Status.Failed;
                status.UpdateMsg = "Cannot remove - events exist in this calendar";
            }
            else
            {
                status = await typeService.DeleteTypeAsync(Id)
                    ? (Status.Success, "Successfully deleted")
                    : (Status.Failed, "An error occured");
            }

            return RedirectToRoute(Url.Settings(status.UpdateResponse, status.UpdateMsg));
        }

        [ValidateAntiForgeryToken()]
        [HttpPost]
        public async Task<ActionResult> AddType(TypeVM dto)
        {
            (Status? UpdateResponse, string UpdateMsg) status = (null, null);

            var insert = new TypeDTO
            {
                Name = dto.Name,
                GroupId = dto.GroupId,
                InviteeIds = (dto.InviteeId != null ? string.Join(",", dto.InviteeId) : null),
                SuperTypeId = dto.Id == 0 ? (int?)null : dto.Id,
                UserCreatedId = dto.UserCreatedId
            };

            status = await typeService.AddTypeAsync(insert)
                ? (Status.Success, "Successfully added")
                : (Status.Failed, "An error occured");

            return RedirectToRoute(Url.Settings(status.UpdateResponse, status.UpdateMsg));
        }

        [ValidateAntiForgeryToken()]
        [HttpPost]
        public async Task<ActionResult> UpdateTags(TagsDTO tags)
        {
            (Status? UpdateResponse, string UpdateMsg) status = (null, null);

            tags.Id = tags.Id.Reverse().Skip(1).Reverse().ToArray();
            tags.Name = tags.Name.Reverse().Skip(1).Reverse().ToArray();
            tags.ThemeColor = tags.ThemeColor.Reverse().Skip(1).Reverse().ToArray();
            tags.TypeID = tags.TypeID.Reverse().Skip(1).Reverse().ToArray();
            tags.UserCreatedId = tags.UserCreatedId.Reverse().Skip(1).Reverse().ToArray();

            var user = await GetUser();
            var tagsA = new Dictionary<int, Tag>();

            int z = 0;
            foreach (var item in tags.Id)
            {
                tagsA.Add(z, new Tag { 
                    Id = item,
                    Name = tags.Name[z],
                    ThemeColor = tags.ThemeColor[z],
                    TypeID = tags.TypeID[z],
                    UserID = tags.UserCreatedId[z]
                });
                z++;
            }

            tags.Tags = new List<Tag>(tagsA.Values);

            status = await UpdateUserTags(tags.Tags, user.UserID)
                ? (Status.Success, "Your tags has been updated successfully")
                : (Status.Failed, "There was an issue updating your tags");

            return RedirectToRoute(Url.Settings(status.UpdateResponse, status.UpdateMsg));
        }
    }
}