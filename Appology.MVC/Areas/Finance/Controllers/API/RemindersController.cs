using Appology.Controllers.Api;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Service;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Appology.Areas.MiFinance.Controllers.API
{
    [RoutePrefix("api/reminders")]
    [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
    [CamelCaseControllerConfig]
    public class RemindersCommonController : ApiController
    {
        private readonly IRemindersService remindersService;
        private readonly IFinanceService financeService;

        public RemindersCommonController(IRemindersService remindersService, IFinanceService financeService)
        {
            this.remindersService = remindersService ?? throw new ArgumentNullException(nameof(remindersService));
            this.financeService = financeService ?? throw new ArgumentNullException(nameof(financeService));
        }

        [HttpGet]
        [Route("")]
        public async Task<HttpResponseMessage> GetRemindersAsync()
        {
            var reminders = await remindersService.GetAllAsync();
            return Request.CreateResponse(HttpStatusCode.OK, new { Reminders = reminders });
        }

        [Route("add")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddReminderAsync(ReminderDTO dto)
        {
            await remindersService.AddReminder(dto);
            return Request.CreateResponse(HttpStatusCode.OK, true);
        }

        [Route("hide/{id}")]
        [HttpPost]
        public async Task<HttpResponseMessage> HideReminderAsync(int id)
        {
            await remindersService.HideReminder(id);
            return Request.CreateResponse(HttpStatusCode.OK, true);
        }

        [Route("notifications")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetNotificationsAsync()
        {
            var notifications = await financeService.GetNotifications();
            var summary = await financeService.GetSummary();

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Notifications = new
                {
                    OverDueReminders = notifications.OverDueReminders.Select(x => new
                    {
                        x.Id,
                        x.Notes,
                        DueDate = x.DueDate.HasValue ? x.DueDate.Value.ToString("d/MM/yyyy HH:mm:ss") : null,
                        x.DaysUntilDue,
                        x.PaymentStatus,
                        x.Priority,
                        x.Category,
                        x.Sort
                    }),
                    UpcomingReminders = notifications.UpcomingReminders.Select(x => new
                    {
                        x.Id,
                        x.Notes,
                        DueDate = x.DueDate.HasValue ? x.DueDate.Value.ToString("d/MM/yyyy HH:mm:ss") : null,
                        x.DaysUntilDue,
                        x.PaymentStatus,
                        x.Priority,
                        x.Category,
                        x.Sort
                    }),
                    DueTodayReminders = notifications.DueTodayReminders.Select(x => new
                    {
                        x.Id,
                        x.Notes,
                        x.PaymentStatus,
                        x.Priority,
                        x.Category,
                        x.Sort
                    }),
                    notifications.Alerts,
                    Summary = summary
                }
            });
        }
    }
}
