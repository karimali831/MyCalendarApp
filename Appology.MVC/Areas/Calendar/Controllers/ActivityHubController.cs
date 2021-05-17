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

            var dateFilter = new ActivityHubDateFilter
            {
                Frequency = DateFrequency.LastXMonths,
                Interval = 1
            };

            var activities = await activityHubService.GetActivities(baseVM.User, dateFilter);
            var activityHub = await activityHubService.GetAllByUserIdAsync(baseVM.User.UserID, dateFilter);

            return View(
                new ActivityHubVM
                {
                    UserTags = await UserTags(baseVM.User.UserID),
                    Filter = dateFilter,
                    Activities = activities,
                    ActivityHub = activityHub,
                    PrevMonthName = DateUtils.DateTime().AddMonths(-1).ToString("MMMM"),
                    PrevMonthNameAbbrev = DateUtils.DateTime().AddMonths(-1).ToString("MMM"),
                    PrevSecondMonthName = DateUtils.DateTime().AddMonths(-2).ToString("MMMM"),
                    PrevSecondMonthNameAbbrev = DateUtils.DateTime().AddMonths(-2).ToString("MMM")
                });
        }

        public async Task<JsonResult> Add(Guid tagId, int value, string dateStr)
        {
            await BaseViewModel(new MenuItem { ActivityHub = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            DateTime date;
            if (DateTime.TryParse(dateStr, out DateTime parsedDate))
            {
                date = parsedDate;
            }
            else
            {
                date = DateUtils.UtcDateTime();
            }

            var model = new ActivityHub
            {
                Id = Guid.NewGuid(),
                UserId = baseVM.User.UserID,
                TagId = tagId,
                Value = value,
                Date =  date
            };

            var status = await activityHubService.AddAsync(model);
            return new JsonResult { Data = status, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public async Task<JsonResult> Delete(Guid Id)
        {
            await BaseViewModel(new MenuItem { ActivityHub = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            var get = await activityHubService.GetAsync(Id);

            if (get == null || baseVM.User.UserID != get.UserId)
            {
                throw new ApplicationException("An error occured deleting hub activity");
            }

            var status = await activityHubService.DeleteAsync(Id);
            return new JsonResult { Data = status, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public async Task<JsonResult> Filter(DateFrequency frequency, int interval, DateTime? fromDate = null, DateTime? toDate = null)
        {
            await BaseViewModel(new MenuItem { ActivityHub = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            var dateFilter = new ActivityHubDateFilter
            {
                Frequency = frequency,
                Interval = interval,
                FromDateRange = fromDate,
                ToDateRange = toDate
            };

            string prevMonthName = DateUtils.DateTime().AddMonths(-1).ToString("MMMM");
            string prevMonthNameAbbrev = DateUtils.DateTime().AddMonths(-1).ToString("MMM");
            string prevSecondMonthName = DateUtils.DateTime().AddMonths(-2).ToString("MMMM");
            string prevSecondMonthNameAbbrev = DateUtils.DateTime().AddMonths(-2).ToString("MMM");


            var stats = await activityHubService.GetActivities(baseVM.User, dateFilter, cacheRemove: true);
            var activityHub = await activityHubService.GetAllByUserIdAsync(baseVM.User.UserID, dateFilter, cacheRemove: true);

            string hubHtml = "";

            foreach (var act in activityHub)
            {
                hubHtml += "<tr>";

                hubHtml += $"<td><span class='fas fa-tag' style='color: {act.ThemeColor}'></span> {act.Subject}</td>";

                hubHtml += $"<td>{act.Value} {(act.TargetUnit == "hours" ? "minutes" : act.TargetUnit)}</td>";

                hubHtml += $"<td>{DateUtils.GetPrettyDate(act.Date)}</td>";

                hubHtml += $"<td data-model-id='{act.Id}' onclick='deleteActivity(this)'> <div class='deleting-{act.Id}' style='display: none'> <div class='loader loader-small'></div></div><div class='delete-{act.Id}'> <i class='fas fa-times'></i> </div></td>";

                hubHtml += $"</tr>";
            }

            string html = "";

            foreach (var tagGroup in stats)
            {
                if (!string.IsNullOrEmpty(tagGroup.Key.TagGroupName))
                {
                    string key = Utils.RemoveSpecialCharacters($"{tagGroup.Key.TagGroupName}{tagGroup.Key.TagGroupdId}");
                    html += $"<div class='list-group'><a href='#{key}' class='list-group-item' data-toggle='collapse'><i class='fas fa-chevron-down'></i> {tagGroup.Key.TagGroupName}";

                    if (tagGroup.Value.Count(x => x.TagGroupId == tagGroup.Key.TagGroupdId && x.TargetUnit == "hours") > 1)
                    {
                        html += $"<span class='float-right' style='color: #000; font-size: small'><i class='fas fa-clock'></i> {tagGroup.Key.Text}</span>";
                    }

                    html += $"</a><div class='list-group in collapse show' id='{key}'>";

                    foreach (var e in tagGroup.Value.Where(x => x.TagGroupId == tagGroup.Key.TagGroupdId))
                    {
                        html += $"<div class='list-group-item'><span class='fas {e.ActivityTag}' style='color: {e.Color}'></span> <small> {e.Text}</small>";

                        if (e.ProgressBar.TargetValue.HasValue && e.ProgressBar.ProgressBarPercentage > 0)
                        {
                            html += $"<div>";

                            html += $"<div class='ah-stats'>";

                            if (e.PreviousSecondMonthTotalValue > 0)
                            {
                                html += $"<span class='ah-badge badge badge-{(e.PreviousSecondMonthSuccess ? "success" : "danger")}'> <i class='fas fa-arrow-{(e.PreviousSecondMonthSuccess ? "up" : "down")}'></i> <span class='prev-secondmonth-desktop'>{e.PreviousSecondMonthTotalValue} {e.TargetUnit} in {prevSecondMonthName}</span> <span class='prev-secondmonth-mobile'>{prevSecondMonthNameAbbrev} {e.PreviousSecondMonthTotalValue}</span> </span>";
                            }

                            if (e.PreviousMonthTotalValue > 0)
                            {
                                html += $"<span class='ah-badge badge badge-{(e.PreviousMonthSuccess ? "success" : "danger")}'> <i class='fas fa-arrow-{(e.PreviousMonthSuccess ? "up" : "down")}'></i> <span class='prev-month-desktop'>{e.PreviousMonthTotalValue} {e.TargetUnit} in {prevMonthName}</span> <span class='prev-month-mobile'>{prevMonthNameAbbrev} {e.PreviousMonthTotalValue}</span> </span>";
                            }

                            html += $"<span class='ah-badge badge badge-{(e.LastWeekSuccess ? "success" : "danger")}'> <i class='fas fa-arrow-{(e.LastWeekSuccess ? "up" : "down")}'></i> <span class='prev-month-desktop'>{e.LastWeekTotalValue} {e.TargetUnit} previous week</span> <span class='prev-month-mobile'>PW {e.LastWeekTotalValue}</span> </span>";
                            
                            html += $"<span class='ah-badge badge badge-info'> <i class='fas fa-clock'></i> <span class='this-week-desktop'>{e.ThisWeekTotalValue} {e.TargetUnit} current week</span> <span class='this-week-mobile'>CW {e.ThisWeekTotalValue}</span> </span>";

                            html += $"<span class='ah-badge badge badge-primary'> <i class='fas fa-bullseye'></i> <span class='target-desktop'>{e.ProgressBar.TargetFrequency} Target {e.ProgressBar.TargetValue} {e.ProgressBar.TargetUnit}</span> <span class='target-mobile'>Target {e.ProgressBar.TargetValue}</span> </span>";

                            html += $"</div>";

                            html += $"<div class='progress' style='height: 20px'> <div class='progress-bar progress-bar-striped progress-bar-animated {e.ProgressBar.ProgressBarColor}' role='progressbar' aria-valuenow='{e.ProgressBar.ActualValue}' aria-valuemin='0' aria-valuemax='{e.ProgressBar.TargetValue.Value}' style='width: {e.ProgressBar.ProgressBarPercentage}%; padding: 10px'> Averaging {e.ProgressBar.ActualValue} {e.ProgressBar.TargetUnit} {e.ProgressBar.TargetFrequency.ToString().ToLower()} </div></div>";

                            html += $"</div>";
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

            return new JsonResult { 
                Data = new
                {
                    stats = Content(html),
                    hub = Content(hubHtml)
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}