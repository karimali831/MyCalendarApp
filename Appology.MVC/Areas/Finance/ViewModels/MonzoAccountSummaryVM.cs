using Appology.MiFinance.Enums;
using Appology.MiFinance.Models;
using Appology.Website.ViewModels;
using System;
using System.Collections.Generic;

namespace Appology.Website.Areas.MiFinance.ViewModels
{
    public class MonzoAccountSummaryVM
    {
        public string Token { get; set; }
        public bool ShowPotAndTags { get; set; }
        public string SortCode { get; set; }
        public string AccountNo { get; set; }
        public decimal SpentToday { get; set; }
        public IDictionary<MonzoTransactionStatus, IList<MonzoTransaction>> Transactions { get; set; }
        public IDictionary<CategoryType, (IList<string> Transactions, string Syncables)> SyncedTransactions { get; set; }
        public decimal Balance { get; set; }
        public decimal SavingsBalance { get; set; }
        public BootBox Modal { get; set; }
        public DateTime LastSynced { get; set; }
    }
}