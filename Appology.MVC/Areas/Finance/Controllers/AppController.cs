using Appology.Controllers;
using Appology.MiFinance.Enums;
using Appology.MiFinance.Model;
using Appology.MiFinance.Service;
using Appology.Service;
using Appology.Website.Areas.MiFinance.ViewModels;
using Appology.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Appology.Areas.MiFinance.Controllers
{
    #if !DEBUG
    [RequireHttps] //apply to all actions in controller
    #endif
    public class AppController : UserMvcController
    {
        private readonly IBaseService baseService;

        public AppController(
            IBaseService baseService,
            IUserService userService,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {
            this.baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
        }

        public async Task<ActionResult> Home()
        {
            await BaseViewModel(new MenuItem { FinanceApp = true });
            return View();
        }

        public async Task<ActionResult> Index()
        {
            await BaseViewModel(new MenuItem { FinanceApp = true });
            return View();
        }

        public async Task<ActionResult> AddSpending()
        {
            await BaseViewModel(new MenuItem { FinanceApp = true });
            return View("Index");
        }

        public async Task<ActionResult> Income()
        {
            await BaseViewModel(new MenuItem { FinanceApp = true });
            return View("Index");
        }

        public async Task<ActionResult> Finance()
        {
            await BaseViewModel(new MenuItem { FinanceApp = true });
            return View("Index");
        }

        public async Task<ActionResult> Category()
        {
            await BaseViewModel(new MenuItem { FinanceApp = true });
            return View("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Settings(Setting model)
        {
            await baseService.UpdateSettingsAsync(model);
            return RedirectToAction("Settings");
        }

        public async Task<ActionResult> Settings()
        {
            await BaseViewModel(new MenuItem { FinanceSettings = true });
            var settings = await baseService.GetSettingsAsync();
            return View(settings);
        }

        public async Task<ActionResult> Categories()
        {
            await BaseViewModel(new MenuItem { FinanceCategories = true });
            var spendingCategories = (await baseService.GetAllCategories(CategoryType.Spendings, catsWithSubs: false));
            var incomeCategories = (await baseService.GetAllCategories(CategoryType.IncomeSources, catsWithSubs: false));

            var viewmodel = new CategoriesVM
            {
                SpendingCategories = spendingCategories,
                IncomeCategories = incomeCategories
            };

            var spendingSecondCats = new Dictionary<CategoryType, IEnumerable<Category>>();
            var incomeSecondCats = new Dictionary<CategoryType, IEnumerable<Category>>();

            foreach (var cat in spendingCategories)
            {
                if (cat.SuperCatId.HasValue)
                {
                    cat.SuperCategory = await baseService.GetCategoryName(cat.SuperCatId.Value);
                }

                if (cat.SecondTypeId.HasValue && cat.SecondTypeId != 0)
                {
                    var secondCategories = await baseService.GetAllCategories(cat.SecondTypeId, catsWithSubs: false);

                    foreach (var cat2 in secondCategories)
                    {
                        if (cat2.SuperCatId.HasValue)
                        {
                            cat2.SuperCategory = await baseService.GetCategoryName(cat2.SuperCatId.Value);
                        }
                    }

                    spendingSecondCats.Add(cat.SecondTypeId.Value, secondCategories);

                }
            }

            foreach (var cat in incomeCategories.Where(x => x.SecondTypeId.HasValue && x.SecondTypeId != 0))
            {
                var secondCategories = await baseService.GetAllCategories(cat.SecondTypeId, catsWithSubs: false);
                incomeSecondCats.Add(cat.SecondTypeId.Value, secondCategories);
            }

            viewmodel.SpendingSecondCategories = spendingSecondCats;
            viewmodel.IncomeSecondCategories = incomeSecondCats;

            return View("Categories", viewmodel);
        }
    }
}