using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MyCalendar.Service
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        IEnumerable<Cronofy.Calendar> GetCalendars();
        bool LoadUser(int passcode);
        Task<User> GetAsync(int passcode);
        Task<bool> UpdateAsync(User user);
        Task<User> GetByUserIDAsync(Guid userID);
        Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags, Guid userID);
        Task<Tag> GetUserTagAysnc(Guid tagID);
        Task<List<string>> CurrentUserActivity(IEnumerable<Event> events);
        Task<IList<User>> GetUsers();
        Task<User> GetUser(int? passcode = null);
        Task<IEnumerable<Tag>> GetUserTags();
        //Task DoWorkAsync();
    }
    ;
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly ITagService tagService;
        private readonly ICronofyService cronofyService;
        private readonly string AuthenticationName = "iCalendarApp-Authentication";

        public UserService(ITagService tagService, IUserRepository userRepository, ICronofyService cronofyService)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            this.cronofyService = cronofyService ?? throw new ArgumentNullException(nameof(cronofyService));
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await userRepository.GetAllAsync();
        }

        public async Task<User> GetAsync(int passcode)
        {
            return await userRepository.GetAsync(passcode); 
        }

        public async Task<bool> UpdateAsync(User user)
        {
            return await userRepository.UpdateAsync(user);
        }

        public async Task<User> GetByUserIDAsync(Guid userID)
        {
            return await userRepository.GetByUserIDAsync(userID);
        }

        public bool LoadUser(int passcode)
        {
            var user = userRepository.Get(passcode);
            return cronofyService.LoadUser(user);
        }

        public async Task<Tag> GetUserTagAysnc(Guid tagID)
        {
            return await tagService.GetAsync(tagID);
        }

        public async Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags, Guid userID)
        {
            return await tagService.UpdateUserTagsAsync(tags, userID);
        }

        public async Task<User> GetUser(int? passcode = null)
        {
            var appCookie = HttpContext.Current.Request.Cookies.Get(AuthenticationName);

            if (passcode.HasValue)
            {
                return await GetAsync(passcode.Value);
            }
            else
            {
                if (appCookie != null)
                {
                    var user = await GetAsync(int.Parse(appCookie.Value));

                    if (user != null)
                    {
                        user.Authenticated = true;

                        if (!string.IsNullOrEmpty(user.CronofyUid) && !string.IsNullOrEmpty(user.DefaultCalendar))
                        {
                            user.CronofyReady = true;
                        }

                        return user;
                    }
                }
            }

            return null;
        }

        public async Task<IList<User>> GetUsers()
        {
            var users = await GetAllAsync();
            var user = await GetUser();

            if (user != null)
            {
                users = users.Where(x => x.UserID != user.UserID);
            }

            return users.ToList();
        }

        public async Task<IEnumerable<Tag>> GetUserTags()
        {
            var user = await GetUser();

            if (user == null)
            {
                return Enumerable.Empty<Tag>();
            }

            var userTags = await tagService.GetUserTagsAsync(user.UserID);

            if (userTags != null)
            {
                return userTags;
            }

            return Enumerable.Empty<Tag>();
        }

        public async Task<List<string>> CurrentUserActivity(IEnumerable<Event> events)
        {
            var currentActivity = new List<string>();

            if (events != null && events.Any())
            {
                foreach (var activity in events)
                {
                    string getName = (await GetByUserIDAsync(activity.UserID)).Name;
                    string label = (await GetUserTagAysnc(activity.TagID))?.Name ?? activity.Description;
                    string finishing = (activity.EndDate.HasValue ? "finishing " + Utils.FromUtcToTimeZone(activity.EndDate.Value).ToString("HH:mm") : "for the day");
                    string starting = Utils.FromUtcToTimeZone(activity.StartDate).ToString("HH:mm");

                    if (Utils.DateTime() >= Utils.FromUtcToTimeZone(activity.StartDate.AddHours(-4)) && Utils.DateTime() < Utils.FromUtcToTimeZone(activity.StartDate))
                    {
                        currentActivity.Add(string.Format("{0} has an upcoming event today - {1} starting {2}", getName, label, starting));
                    }
                    else
                    {
                        currentActivity.Add(string.Format("{0} currently at event - {1} {2}", getName, label, finishing));
                    }
                }
            }

            return currentActivity;
        }



        public IEnumerable<Cronofy.Calendar> GetCalendars()
        {
            return cronofyService.GetCalendars();
        }


        //public Task DoWorkAsync() // No async because the method does not need await
        //{
        //    return Task.Run(() =>
        //    {
        //        SendMail("Hello world", "karimali831@googlemail.com", "TEST");
        //    });
        //}

        //public static bool SendMail(string subject, string to, string body)
        //{
        //    string fromMailAddress = ConfigurationManager.AppSettings["MailAddress"];
        //    string fromMailPassword = ConfigurationManager.AppSettings["MailPassword"];
        //    string fromMailName = ConfigurationManager.AppSettings["MailName"];

        //    fromMailAddress = "admin@karim-ali.co.uk";
        //    fromMailPassword = "Xra63400*";
        //    fromMailName = "iCalendarApp";


        //    var networkConfig = new NetworkCredential(fromMailAddress, fromMailPassword);
        //    var mailServer = new SmtpClient()
        //    {
        //        //Host = ConfigurationManager.AppSettings["SmtpHost"],
        //        Host = "mail.karim-ali.co.uk",
        //        UseDefaultCredentials = false,
        //        Credentials = networkConfig
        //    };
        //    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SmtpPort"]))
        //        mailServer.Port = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);

        //    mailServer.Port = 465;

        //    var message = new MailMessage()
        //    {
        //        Subject = subject,
        //        SubjectEncoding = Encoding.UTF8,
        //        IsBodyHtml = true,
        //        BodyEncoding = Encoding.UTF8,
        //    };

        //    //message send config
        //    message.To.Add(new MailAddress(to));
        //    message.From = new MailAddress(fromMailAddress, fromMailName);
        //    message.Body = body;

        //    try
        //    {
        //        mailServer.SendAsync(message, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }

        //    return true;
        //}
    }
}
