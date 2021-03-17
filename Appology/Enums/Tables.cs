using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Appology.Enums
{
    public enum Table
    {
        // common
        Categories,
        FeatureRoles,
        Features,
        Groups,
        Roles,
        Users,
        Notifications,
        // finance,
        FinanceCategories,
        Finances,
        Incomes,
        MonzoAccount,
        MonzoTransactions,
        Reminders,
        Settings,
        Spendings,
        // errand runner
        Orders,
        Stakeholders,
        Trips,
        // calendar & write
        Events, 
        Documents,
        DocumentChangelog,
        Tags,
        Types
    }

    public static class Tables
    {
        public static string Name(Table table)
        {
            return TableNames().First(x => x.Key == table).Value;
        }

        private static IList<KeyValuePair<Table, string>> TableNames()
        {
            return new List<KeyValuePair<Table, string>>() {
                // common
                new KeyValuePair<Table, string>(Table.Categories, "Categories"),
                new KeyValuePair<Table, string>(Table.FeatureRoles, "FeatureRoles"),
                new KeyValuePair<Table, string>(Table.Features, "Features"),
                new KeyValuePair<Table, string>(Table.Groups, "Groups"),
                new KeyValuePair<Table, string>(Table.Roles, "Roles"),
                new KeyValuePair<Table, string>(Table.Types, "Types"),
                new KeyValuePair<Table, string>(Table.Users, "Users"),
                new KeyValuePair<Table, string>(Table.Notifications, "Notifications"),
                // errand runner
                new KeyValuePair<Table, string>(Table.Orders, "[ER.Orders]"),
                new KeyValuePair<Table, string>(Table.Stakeholders, "[ER.Stakeholders]"),
                new KeyValuePair<Table, string>(Table.Trips, "[ER.Trips]"),
                // calendar
                new KeyValuePair<Table, string>(Table.Tags, "Tags"),
                new KeyValuePair<Table, string>(Table.Events, "Events"),
                // finance
                new KeyValuePair<Table, string>(Table.FinanceCategories, "[Finance.Categories]"),
                new KeyValuePair<Table, string>(Table.Finances, "[Finance.Finances]"),
                new KeyValuePair<Table, string>(Table.Incomes, "[Finance.Incomes]"),
                new KeyValuePair<Table, string>(Table.MonzoAccount, "[Finance.MonzoAccount]"),
                new KeyValuePair<Table, string>(Table.MonzoTransactions, "[Finance.MonzoTransactions]"),
                new KeyValuePair<Table, string>(Table.Reminders, "[Finance.Reminders]"),
                new KeyValuePair<Table, string>(Table.Settings, "[Finance.Settings]"),
                new KeyValuePair<Table, string>(Table.Spendings, "[Finance.Spendings]"),
                // write
                new KeyValuePair<Table, string>(Table.Documents, "Documents"),
                new KeyValuePair<Table, string>(Table.DocumentChangelog, "DocumentChangelog"),
            };
        }
    }
}
