using Appology.MiFinance.Enums;
using Appology.MiFinance.Model;
using System.Collections.Generic;

namespace Appology.Website.Areas.MiFinance.ViewModels
{
    public class CategoriesVM
    {
        public IEnumerable<Category> SpendingCategories { get; set; }
        public IDictionary<CategoryType, IEnumerable<Category>> SpendingSecondCategories { get; set; }
        public IEnumerable<Category> IncomeCategories { get; set; }
        public IDictionary<CategoryType, IEnumerable<Category>> IncomeSecondCategories { get; set; }
    }
}