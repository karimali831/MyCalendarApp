using Appology.MiFinance.DTOs;
using Appology.MiFinance.Enums;
using Appology.MiFinance.Model;
using Appology.MiFinance.Repository;
using Appology.MiFinance.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.MiFinance.Service
{
    public interface IRemindersService
    {
        Task<IEnumerable<Reminder>> GetAllAsync();
        Task AddReminder(ReminderDTO dto);
        Task HideReminder(int Id);
        Task<bool> ReminderExists(string notes);
        Task MissedEntriesAsync(IList<MissedEntries> entries, string notes, Priority priority = Priority.Low);
    }

    public class RemindersService : IRemindersService
    {
        private readonly IRemindersRepository remindersRepository;

        public RemindersService(IRemindersRepository remindersRepository)
        {
            this.remindersRepository = remindersRepository ?? throw new ArgumentNullException(nameof(remindersRepository));
        }

        public async Task<IEnumerable<Reminder>> GetAllAsync()
        {
            var reminders = await remindersRepository.GetAllAsync();

            if (reminders != null && reminders.Any())
            {
                foreach (var reminder in reminders)
                {
                    if (reminder.DueDate != null)
                    {
                        if (reminder.DueDate?.Date < DateTime.UtcNow.Date)
                        {
                            reminder.PaymentStatus = PaymentStatus.Late;
                        }
                        else if (reminder.DueDate?.Date == DateTime.UtcNow.Date)
                        {
                            reminder.PaymentStatus = PaymentStatus.DueToday;
                        }
                        else if (reminder.DueDate <= DateTime.UtcNow.Date.AddDays(7) && reminder.DueDate?.Date > DateTime.UtcNow.Date)
                        {
                            reminder.PaymentStatus = PaymentStatus.Upcoming;
                        }
                    }
                    else
                    {
                        reminder.PaymentStatus = PaymentStatus.Unknown;
                    }
                }
            }

            return reminders;
        }
      
        public async Task AddReminder(ReminderDTO dto)
        {
            if (!await ReminderExists(dto.Notes))
            {
                await remindersRepository.InsertAsync(dto);
            }
        }

        public async Task HideReminder(int Id)
        {
            await remindersRepository.HideAsync(Id);
        }

        public async Task<bool> ReminderExists(string notes)
        {
            return await remindersRepository.ReminderExists(notes);
        }

        public async Task MissedEntriesAsync(IList<MissedEntries> entries, string notes, Priority priority = Priority.Low)
        {
            if (entries.Any())
            {
                foreach (var entry in entries)
                {
                    if (entry.Dates.Any())
                    {
                        foreach (var missedDates in entry.Dates)
                        {
                            string dbNotes = string.Format("{0} {1}. ({2})", notes, entry.Name, missedDates);

                            await AddReminder(new ReminderDTO
                            {
                                DueDate = DateTime.UtcNow,
                                Notes = dbNotes,
                                Priority = priority,
                                CatId = Categories.MissedEntries
                            });
                        }
                    }
                }
            }
        }
    }
}
