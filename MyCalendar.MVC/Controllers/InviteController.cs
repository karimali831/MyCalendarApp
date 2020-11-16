using MyCalendar.Controllers;
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Website.Controllers
{
    public class InviteController : UserMvcController
    {
        public InviteController(IUserService userService, IFeatureRoleService featureRoleService) : base(userService, featureRoleService)
        {

        }
        public async Task<ActionResult> User(Guid id)
        {
            (Status? UpdateResponse, string UpdateMsg) status = (null, null);

            if (id == Guid.Empty)
            {
                status.UpdateResponse = Status.Failed;
                status.UpdateMsg = "Invalid inviter share link";
            }
            else
            {
                await BaseViewModel(new MenuItem { None = true });
                var baseVM = ViewData["BaseVM"] as BaseVM;

                var inviter = await GetUserById(id);

                if (baseVM.User.UserID == id)
                {
                    status.UpdateResponse = Status.Failed;
                    status.UpdateMsg = "You cannot add yourself as a buddy";
                }
                else if (!(await GetUsers()).Any(x => x.UserID == id))
                {
                    status.UpdateResponse = Status.Failed;
                    status.UpdateMsg = "Could not find user";
                }
                else  
                {
                    var inviterBuddyList = await GetBuddys(id);

                    if (baseVM.Buddys != null && baseVM.Buddys.Any(x => x.UserID == id))
                    {
                        status.UpdateResponse = Status.Failed;
                        status.UpdateMsg = $"{inviter.Name} is already on your buddy-list";
                    }
                    else if (inviterBuddyList != null && inviterBuddyList.Any(x => x.UserID == id))
                    {
                        status.UpdateResponse = Status.Failed;
                        status.UpdateMsg = $"You are already on {inviter.Name}'s buddy-list";
                    }
                    else
                    {
                        var inviteeBuddyIds = new List<Guid>();

                        // add to invitees buddy list
                        if (!string.IsNullOrEmpty(baseVM.User.BuddyIds))
                        {
                            inviteeBuddyIds.Add(id);
                            baseVM.User.BuddyIds = string.Join(",", inviteeBuddyIds.Concat(baseVM.Buddys.Select(x => x.UserID)));
                        }
                        else
                        {
                            baseVM.User.BuddyIds = id.ToString();
                        }

                        var inviterBuddyIds = new List<Guid>();

                        // to add inviters buddy-list
                        if (!string.IsNullOrEmpty(inviter.BuddyIds))
                        {
                            inviterBuddyIds.Add(baseVM.User.UserID);
                            inviter.BuddyIds = string.Join(",", inviterBuddyIds.Concat(inviterBuddyList.Select(x => x.UserID)));
                        }
                        else
                        {
                            inviter.BuddyIds = baseVM.User.UserID.ToString();
                        }

                        var updateInvitee = await UpdateUser(baseVM.User);
                        var updateInviter = await UpdateUser(inviter);

                        status = (updateInvitee && updateInviter)
                            ? (Status.Success, $"You and {inviter.Name} are now buddys")
                            : (Status.Failed, $"There was an issue adding you or {inviter.Name}'s as a buddy");
                    }
                }
            }
            
            return RedirectToRoute(Url.Settings(status.UpdateResponse, status.UpdateMsg));
        }

        public async Task<ActionResult> Remove(Guid id)
        {
            (Status? UpdateResponse, string UpdateMsg) status = (null, null);
            await BaseViewModel(new MenuItem { None = true });

            var baseVM = ViewData["BaseVM"] as BaseVM;
            var removee = await GetUserById(id);
            var removeeBuddys = (await GetBuddys(removee.UserID)).ToList();

            baseVM.User.BuddyIds = string.Join(",", baseVM.Buddys
                .Where(x => x.UserID != id)
                .Select(x => x.UserID)) ?? null;

            removee.BuddyIds = string.Join(",", removeeBuddys
                .Where(x => x.UserID != baseVM.User.UserID)
                .Select(x => x.UserID)) ?? null;

            var updateRemovee = await UpdateUser(removee);
            var updateRemover = await UpdateUser(baseVM.User);

            status = (updateRemovee && updateRemover)
                ? (Status.Success, $"You and {removee.Name} are no longer buddys")
                : (Status.Failed, $"There was an issue removing you or {removee.Name}'s as a buddy");

            return RedirectToRoute(Url.Settings(status.UpdateResponse, status.UpdateMsg));
        }
    }
}