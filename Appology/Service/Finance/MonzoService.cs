using Appology.Enums;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Enums;
using Appology.MiFinance.Helpers;
using Appology.MiFinance.Model;
using Appology.MiFinance.Models;
using Appology.MiFinance.Repository;
using Appology.MiFinance.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Categories = Appology.MiFinance.Enums.Categories;

namespace Appology.MiFinance.Service
{
    public interface IMonzoService
    {
        Task<IDictionary<CategoryType, (IList<string>, string Syncables)>> SyncTransactions(IList<MonzoTransaction> transactions);
        Task<IEnumerable<FinanceVM>> GetFinances();
        Task<IEnumerable<Category>> GetCategories(CategoryType? typeId);
        Task AddIncome(IncomeDTO dto);
        Task AddSpending(SpendingDTO dto);
        Task<Monzo> MonzoAccountSummary();
        Task InsertMonzoAccountSummary(Monzo accountSummary);
        Task UpdateCashBalanceAsync(decimal cashBalance);
        Task InsertMonzoTransaction(MonzoTransaction transaction);
        Task<IList<MonzoTransaction>> MonzoTransactions();
        Task<bool> TransactionExists(string transId);
        Task DeleteMonzoTransaction(string transId);
        Task<bool> SpendingTransactionExists(string transId);
    }

    public class MonzoService : IMonzoService
    {
        private readonly IMonzoRepository monzoRepository;
        private readonly IFinanceService financeService;
        private readonly ISpendingService spendingService;
        private readonly IIncomeService incomeService;
        private readonly IBaseService baseService;
        private readonly IRemindersService reminderService;
        private readonly ISettingRepository settingRepository;

        public MonzoService(
            IMonzoRepository monzoRepository,
            IFinanceService financeService,
            ISpendingService spendingService,
            IIncomeService incomeService,
            IRemindersService reminderService,
            ISettingRepository settingRepository,
            IBaseService baseService)
        {
            this.monzoRepository = monzoRepository ?? throw new ArgumentNullException(nameof(monzoRepository));
            this.financeService = financeService ?? throw new ArgumentNullException(nameof(financeService));
            this.spendingService = spendingService ?? throw new ArgumentNullException(nameof(spendingService));
            this.incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
            this.baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
            this.reminderService = reminderService ?? throw new ArgumentNullException(nameof(reminderService));
            this.settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
        }

        private async Task CheckMonzoTransDuplicates()
        {
            var spendingDuplicates = await baseService.CheckDuplicates("MonzoTransId", Table.Spendings);
            var incomeDuplicates = await baseService.CheckDuplicates("MonzoTransId", Table.Incomes);
            var duplicates = spendingDuplicates.Concat(incomeDuplicates);

            if (duplicates != null && duplicates.Any())
            {
                foreach (var (Name, Duplicates) in duplicates.Where(x => x.Duplicates >= 2))
                {
                    string note = $"Duplicated monzo trans entries: {Duplicates} for {Name}";
      
                    await reminderService.AddReminder(new ReminderDTO
                    {
                        DueDate = DateTime.UtcNow,
                        Notes = note,
                        Priority = Priority.Medium,
                        CatId = Categories.MissedEntries,
                        MonzoTransId = Name
                    });
                    
                }
            }
        }

        private async Task MonzoTagMismatched(string transName, string cat = null, string secondCat = null)
        {
            string msg = "";
            if (secondCat != null)
            {
                msg = $"Unable to automatically sync expense transaction: {transName} with matched category {cat} and unmatched second category: {secondCat}";
            }
            else if (cat != null)
            {
                msg = $"Unable to automatically sync expense transaction: {transName} with unmatched category: {cat}";
            }
            else
            {
                msg = $"Unable to automatically sync income transaction: {transName}";
            }

            await reminderService.AddReminder(new ReminderDTO
            {
                DueDate = DateTime.UtcNow,
                Notes = msg,
                Priority = Priority.Medium,
                CatId = Categories.MonzoTransaction
            });
            
        }

        private async Task<(int? CatId, int? SecondCatId, string cat1Name)> AutosyncWithMonzoTags(MonzoTransaction trans, IEnumerable<Category> categories)
        {
            int? category = null;
            int? secondCategory = null;
            string name = null;

            var tranNotesCategories = trans.Notes.Split('#');
            var tag1 = FinanceUtils.GetUntilOrEmpty(tranNotesCategories[1]);
            var cat1 = categories.FirstOrDefault(x => x.MonzoTag != null && x.MonzoTag.Equals(tag1, StringComparison.OrdinalIgnoreCase));

            if (cat1 != null)
            {
                category = cat1.Id;

                // check if there's a second category 
                if (tranNotesCategories.Length == 3)
                {
                    var tag2 = FinanceUtils.GetUntilOrEmpty(tranNotesCategories[2]);
                    var secondCategories = await baseService.GetAllCategories(cat1.SecondTypeId, catsWithSubs: false);
                    var cat2 = secondCategories.FirstOrDefault(x => x.MonzoTag != null && x.MonzoTag.Equals(tag2, StringComparison.OrdinalIgnoreCase));

                    if (cat2 != null)
                    {
                        secondCategory = cat2.Id;
                    }
                    else
                    {
                        await MonzoTagMismatched(trans.Name, cat1.Name, tag2);
                    }
                }

                name = cat1.Name;
            }
            else
            {
                await MonzoTagMismatched(trans.Name, tag1);
            }

            return (category, secondCategory, name);
        }

        public async Task<IDictionary<CategoryType, (IList<string>, string Syncables)>> SyncTransactions(IList<MonzoTransaction> transactions)
        {
            // variable initialisation 
            var spendingIDs = new List<string>();
            var incomeIDs = new List<string>();
            var incomeSyncables = new List<string>();
            var spendingSyncables = new List<string>();
            DateTime startDate = DateTime.Parse("14/05/2020", new CultureInfo("en-GB")); // start date since started savings pot top-ups

            // check duplicates - should be unit test
            await CheckMonzoTransDuplicates();

            // check synced transactions
            var spendingsMonzoTransIds = await spendingService.RecentMonzoSyncedTranIds(max: 150);
            var incomesMonzoTransIds = await incomeService.RecentMonzoSyncedTranIds(max: 150);
            var categories = (await baseService.GetAllCategories(CategoryType.Spendings, catsWithSubs: false)).Where(x => x.MonzoTag != null);
            var incomeCategories = (await baseService.GetAllCategories(CategoryType.IncomeSources, catsWithSubs: false)).Where(x => x.MonzoTag != null);
            var finances = (await GetFinances()).Where(x => x.MonzoTag != null);

            // 2020-06-01T14:15:00.119Z
            foreach (var trans in transactions)
            {
                // add alert 
                if (trans.Notes.StartsWith("*"))
                {
                    await reminderService.AddReminder(new ReminderDTO
                    {
                        Notes = trans.Notes.Substring(1),
                        Priority = Priority.High,
                        DueDate = DateTime.UtcNow,
                        CatId = Categories.MonzoTransaction
                    });
                }
                else
                {

                    int? category = null;
                    int? secondCategory = null;
                    int? financeId = null;
                    string name = "";

                    // debited
                    if (trans.Amount < 0)
                    {
                        if (spendingsMonzoTransIds.Contains(trans.Id))
                        {
                            spendingIDs.Add(trans.Id);
                        }
                        else
                        {
                            // auto sync spending categories
                            if (trans.Notes.StartsWith("#"))
                            {
                                var getCats = await AutosyncWithMonzoTags(trans, categories);

                                if (getCats.CatId.HasValue)
                                {
                                    category = getCats.CatId.Value;

                                    if (getCats.SecondCatId.HasValue)
                                    {
                                        secondCategory = getCats.SecondCatId.Value;
                                    }

                                    spendingSyncables.Add(getCats.cat1Name);
                                    name = trans.Notes.Contains('*') ? trans.Notes.Split('*').Last() : trans.Name;
                                }
                            }
                            else
                            {
                                // auto sync finances
                                var finance = finances.FirstOrDefault(x => x.MonzoTag != null && x.MonzoTag.Equals(trans.Description, StringComparison.OrdinalIgnoreCase));

                                if (finance != null)
                                {
                                    name = trans.Name;
                                    financeId = finance.Id;
                                    spendingSyncables.Add(name);
                                }

                                // auto sync savings
                                if (trans.Name.StartsWith("pot_"))
                                {
                                    name = "Saving pot top-ups";
                                    category = (int)Categories.Savings;
                                    spendingSyncables.Add(name);
                                }
                            }

                            if (category.HasValue || financeId.HasValue)
                            {
                                var dto = new SpendingDTO
                                {
                                    Name = name,
                                    Amount = (-trans.Amount / 100m),
                                    CatId = category ?? null,
                                    SecondCatId = secondCategory,
                                    Date = trans.Created,
                                    MonzoTransId = trans.Id,
                                    FinanceId = financeId ?? null
                                };

                                await spendingService.InsertAsync(dto);
                            }
                        }

                        // auto sync pot incomes
                        if (!incomesMonzoTransIds.Contains(trans.Id))
                        {
                            if (trans.Name.StartsWith("pot_"))
                            {
                                var dto = new IncomeDTO
                                {
                                    Name = "Saving pot top-ups",
                                    Amount = (-trans.Amount / 100m),
                                    SourceId = (int)Categories.SavingsPot,
                                    Date = trans.Created,
                                    MonzoTransId = trans.Id
                                };

                                incomeSyncables.Add("Saving pot top-ups");
                                await incomeService.InsertIncomeAsync(dto);
                            }
                        }
                    }
                    // credited
                    else if (trans.Amount > 0)
                    {
                        if (incomesMonzoTransIds.Contains(trans.Id))
                        {
                            incomeIDs.Add(trans.Id);
                        }
                        else
                        {
                            // auto sync income categories
                            if (trans.Notes.StartsWith("#"))
                            {
                                var getCats = await AutosyncWithMonzoTags(trans, incomeCategories);

                                if (getCats.CatId.HasValue)
                                {
                                    category = getCats.CatId.Value;

                                    if (getCats.SecondCatId.HasValue)
                                    {
                                        secondCategory = getCats.SecondCatId.Value;
                                    }

                                    incomeSyncables.Add(getCats.cat1Name);
                                    name = trans.Notes.Contains('*') ? trans.Notes.Split('*').Last() : trans.Name;
                                }
                            }
                            else
                            {
                                // auto sync incomes
                                var cat = incomeCategories.FirstOrDefault(x => x.MonzoTag != null && x.MonzoTag.Equals(trans.Description, StringComparison.OrdinalIgnoreCase));

                                if (cat != null)
                                {
                                    name = trans.Name;
                                    category = cat.Id;
                                    incomeSyncables.Add(name);
                                }
                                else
                                {
                                    await MonzoTagMismatched(trans.Name);
                                }
                            }

                            if (category.HasValue)
                            {
                                var dto = new IncomeDTO
                                {
                                    Name = name,
                                    Amount = trans.Amount / 100m,
                                    SourceId = category.Value,
                                    Date = trans.Created,
                                    MonzoTransId = trans.Id
                                };

                                await incomeService.InsertIncomeAsync(dto);
                            }
                        }
                    }
                }
            }

            string incomeSyncablesStr = incomeSyncables.Any() ? "Incomes: " + string.Join(", ", incomeSyncables.Distinct()) : "";
            string spendingSyncablesStr = spendingSyncables.Any() ? "Spendings: " + string.Join(", ", spendingSyncables.Distinct()) : "";

            return new Dictionary<CategoryType, (IList<string>, string)>
            {
                { CategoryType.Spendings, (spendingIDs, spendingSyncablesStr) },
                { CategoryType.Income, (incomeIDs, incomeSyncablesStr) }
            };
        }

        public async Task<IEnumerable<FinanceVM>> GetFinances()
        {
            return await financeService.GetFinances(resyncNextDueDates: false);
        }

        public async Task<IEnumerable<Category>> GetCategories(CategoryType? typeId)
        {
            return await baseService.GetAllCategories(typeId, catsWithSubs: false);
        }

        public async Task AddIncome(IncomeDTO dto)
        {
            await incomeService.InsertIncomeAsync(dto);
        }

        public async Task AddSpending(SpendingDTO dto)
        {
            await spendingService.InsertAsync(dto);
        }

        public async Task<Monzo> MonzoAccountSummary()
        {
            return await monzoRepository.MonzoAccountSummary();
        }

        public async Task<bool> SpendingTransactionExists(string transId)
        {
            return await spendingService.MonzoTransactionExists(transId);
        }

        // currently just unsycned transaction
        public async Task<IList<MonzoTransaction>> MonzoTransactions()
        {
            return (await monzoRepository.MonzoTransactions()).ToList();
        }

        public async Task InsertMonzoAccountSummary(Monzo accountSummary)
        {
            await monzoRepository.InsertMonzoAccountSummary(accountSummary);
        }

        // currently unsynced transactions
        public async Task InsertMonzoTransaction(MonzoTransaction transaction)
        {
            await monzoRepository.InsertMonzoTransaction(transaction);
        }

        public async Task DeleteMonzoTransaction(string transId)
        {
            await monzoRepository.DeleteMonzoTransaction(transId);
        }

        public async Task<bool> TransactionExists(string transId)
        {
            return await monzoRepository.TransactionExists(transId);
        }

        public async Task UpdateCashBalanceAsync(decimal cashBalance)
        {
            await settingRepository.UpdateCashBalanceAsync(cashBalance);
        }


    }
}
