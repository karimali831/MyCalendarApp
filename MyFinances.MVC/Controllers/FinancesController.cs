using Ninject;
using MyFinances.DTOs;
using MyFinances.Service;
using MyFinances.Website.ViewModels.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyFinances.Controllers
{
    public class FinanceController : Controller
    {
        private readonly IFinanceService financeService;

        public FinanceController(IFinanceService financeService)
        {
            this.financeService = financeService ?? throw new ArgumentNullException(nameof(financeService));
        }

        public async Task<ActionResult> Index()
        {
            var Finance = await financeService.GetAllAsync();
            return View(Finance);
        }
    }
}