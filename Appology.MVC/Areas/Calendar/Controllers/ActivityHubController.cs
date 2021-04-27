using Appology.Controllers;
using Appology.MiCalendar.DTOs;
using Appology.Enums;
using Appology.Helpers;
using Appology.MiCalendar.Service;
using Appology.Service;
using Appology.Website.Areas.MiCalendar.ViewModels;
using Appology.Website.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Configuration;
using Appology.MiCalendar.Model;

namespace Appology.Areas.MiCalendar.Controllers
{
    public class ActivityHubController : UserMvcController
    {
        private readonly IActivityHubService activityHubService;

        public ActivityHubController(
            IUserService userService,
            IActivityHubService activityHubService,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {
            this.activityHubService = activityHubService ?? throw new ArgumentNullException(nameof(activityHubService));
        }
         
        public async Task<ActionResult> Index()
        {
            await BaseViewModel(new MenuItem { ActivityHub = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            string currentMonth = DateUtils.DateTime().ToString("MMMM");
            DateFrequency currentFrequency = Utils.ParseEnum<DateFrequency>(currentMonth);

            var dateFilter = new DateFilter
            {
                Frequency = DateFrequency.LastXMonths,
                Interval = 1
            };

            var activities = await activityHubService.GetActivities(baseVM.User, dateFilter);

            return View(
                new ActivityHubVM
                {
                    UserTags = await UserTags(baseVM.User.UserID),
                    Filter = dateFilter,
                    Activities = activities
                });
        }

        public async Task<JsonResult> Add(Guid tagId, int minutes)
        {
            await BaseViewModel(new MenuItem { ActivityHub = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            var model = new ActivityHub
            {
                Id = Guid.NewGuid(),
                UserId = baseVM.User.UserID,
                TagId = tagId,
                Minutes = minutes,
                Date = DateUtils.UtcDateTime()
            };

            var status = await activityHubService.AddAsync(model);
            return new JsonResult { Data = status, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public async Task<JsonResult> Filter(DateFrequency frequency, int interval, DateTime? fromDate = null, DateTime? toDate = null)
        {
            await BaseViewModel(new MenuItem { ActivityHub = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            var dateFilter = new DateFilter
            {
                Frequency = frequency,
                Interval = interval,
                FromDateRange = fromDate,
                ToDateRange = toDate
            };

            var eventsOverview = await activityHubService.GetActivities(baseVM.User, dateFilter, cacheRemove: true);

            string html = "";

            foreach (var tagGroup in eventsOverview)
            {
                if (!string.IsNullOrEmpty(tagGroup.Key.TagGroupName))
                {
                    string key = Utils.RemoveSpecialCharacters($"{tagGroup.Key.TagGroupName}{tagGroup.Key.TagGroupdId}");
                    html += $"<div class='list-group'><a href='#{key}' class='list-group-item' data-toggle='collapse'><i class='fas fa-chevron-down'></i> {tagGroup.Key.TagGroupName}";

                    if (tagGroup.Value.Count(x => x.TagGroupId == tagGroup.Key.TagGroupdId) > 1)
                    {
                        html += $"<span class='float-right' style='color: #000; font-size: small'><i class='fas fa-tags'></i> {tagGroup.Key.Text}</span>";
                    }

                    html += $"</a><div class='list-group in collapse show' id='{key}'>";

                    foreach (var e in tagGroup.Value.Where(x => x.TagGroupId == tagGroup.Key.TagGroupdId))
                    {
                        html += $"<div class='list-group-item'><span class='fas {e.ActivityTag}' style='color: {e.Color}'></span> <small> {e.Text}</small>";

                        if (e.ProgressBarWeeklyHours.TargetWeeklyHours != 0 && e.ProgressBarWeeklyHours.ProgressBarPercentage > 0)
                        {
                            html += $"<div class='progress' style='height: 20px'><div class='progress-bar progress-bar-striped progress-bar-animated {e.ProgressBarWeeklyHours.ProgressBarColor}' role='progressbar' aria-valuenow='{e.ProgressBarWeeklyHours.ActualWeeklyHours}' aria-valuemin='0' aria-valuemax='{e.ProgressBarWeeklyHours.TargetWeeklyHours}' style='width: {e.ProgressBarWeeklyHours.ProgressBarPercentage}%; padding: 10px'>Averaging {e.ProgressBarWeeklyHours.ActualWeeklyHours} / {e.ProgressBarWeeklyHours.TargetWeeklyHours} hours a week</div></div>";
                        }

                        foreach (var avatar in e.Avatars)
                        {
                            if (avatar.Length == 2)
                            {
                                html += $"<p default-avatar='{avatar}' style='width: 24px; height: 24px; float: right'></p>";
                            }
                            else
                            {
                                html += $"<img src='{ConfigurationManager.AppSettings["RootUrl"]}/{avatar}' style='width: 24px; height: 24px; float: right' />";
                            }
                        }

                        html += "</div>";
                    }

                    html += "</div></div>";
                }
            }

            return new JsonResult { Data = Content(html), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}