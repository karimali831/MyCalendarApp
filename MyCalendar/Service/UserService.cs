using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Repository;
using MyCalendar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Configuration;

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
        Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags, Guid userId);
        Task<Tag> GetUserTagAysnc(Guid tagID);
        Task<List<string>> CurrentUserActivity(IEnumerable<Event> events, Guid userId);
        Task<IList<User>> GetUsers(Guid userId);
        Task<User> GetUser(int? passcode = null);
        Task<IEnumerable<Tag>> GetUserTags(Guid userId);
        //Task DoWorkAsync();
    }
    ;
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly ITagService tagService;
        private readonly ICronofyService cronofyService;
        private readonly ITagRepository tagRepository;
        private readonly string AuthenticationName;

        public UserService(ITagService tagService, IUserRepository userRepository, ICronofyService cronofyService, ITagRepository tagRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            this.cronofyService = cronofyService ?? throw new ArgumentNullException(nameof(cronofyService));
            this.tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));

            AuthenticationName = ConfigurationManager.AppSettings["AuthenticationName"];
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
            user.ExtCalendarRights ??= Enumerable.Empty<ExtCalendarRights>();
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

        public async Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags, Guid userId)
        {
            if (tags.Any())
            {
                var userTags = await GetUserTags(userId);
                var deletingTags = userTags.Where(x => x.UserID == userId).Select(x => x.Id).Except(tags.Select(x => x.Id));

                if (userTags.Any() && deletingTags.Any())
                {
                    foreach (var tag in deletingTags)
                    {
                        if (!await tagService.EventsByTagExist(tag))
                        {
                            await tagRepository.DeleteTagByIdAsync(tag);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return await tagRepository.UpdateUserTagsAsync(tags, userId);
            }
            else
            {
                await tagRepository.DeleteAllUserTagsAsync(userId);
                return true;
            }
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

                        if (!user.EnableCronofy)
                        {
                            user.CronofyReady = CronofyStatus.Disabled;
                        }
                        else if (string.IsNullOrEmpty(user.CronofyUid) || string.IsNullOrEmpty(user.AccessToken) || string.IsNullOrEmpty(user.RefreshToken))
                        {
                            user.CronofyReady = CronofyStatus.NotAuthenticated;
                        }
                        else
                        {
                            var getCalendarNames = cronofyService.GetProfiles().Select(x => Utils.UppercaseFirst(x.ProviderName));
                            user.CronofyReadyCalendarName = string.Format("{0} Calendar{1}", string.Join(", ", getCalendarNames), getCalendarNames.Count() > 1 ? "s" : "");

                            if (user.ExtCalendarRights != null && user.ExtCalendarRights.Any(x => x.Read || x.Delete || x.Save))
                            {
                                user.CronofyReady = CronofyStatus.AuthenticatedRightsSet;
                            }
                            else
                            {
                                user.CronofyReady = CronofyStatus.AuthenticatedNoRightsSet;
                            }

                        }

                        return user;
                    }
                }
            }

            return null;
        }

        public async Task<IList<User>> GetUsers(Guid userId)
        {
            return (await GetAllAsync()).Where(x => x.UserID != userId).ToList();
        }

        public async Task<IEnumerable<Tag>> GetUserTags(Guid userId)
        {
            var user = await GetByUserIDAsync(userId);
            var userTags = await userRepository.GetTagsByUserAsync(userId);

            if (userTags != null && userTags.Any())
            {
                foreach (var tag in userTags)
                {
                    tag.UpdateDisabled = tag.UserID != user.UserID ? true : false;
                }

                return userTags;
            }
 
            return Enumerable.Empty<Tag>();
        }

        public async Task<List<string>> CurrentUserActivity(IEnumerable<Event> events, Guid userId)
        {
            var currentActivity = new List<string>();

            if (events != null && events.Any())
            {
                var users = await GetUsers(userId);

                foreach (var activity in events)
                {
                    var tag = await GetUserTagAysnc(activity.TagID);
                    string userName = (await GetByUserIDAsync(activity.UserID)).Name;
                    string getName = "";

                    if (tag != null && tag.Privacy == TagPrivacy.Shared)
                    {
                        getName += $"You, {string.Join(", ", users.Select(x => x.Name))}";
                    }
                    else
                    {
                        getName = (userId == activity.UserID ? "You" : userName);
                    }

                    string label = tag?.Name ?? activity.Description;
                    string finishing = (activity.EndDate.HasValue ? "finishing " + Utils.FromUtcToTimeZone(activity.EndDate.Value).ToString("HH:mm") : "for the day");
                    string starting = Utils.FromUtcToTimeZone(activity.StartDate).ToString("HH:mm");

                    if (Utils.DateTime() >= Utils.FromUtcToTimeZone(activity.StartDate.AddHours(-4)) && Utils.DateTime() < Utils.FromUtcToTimeZone(activity.StartDate))
                    {
                        string pronoun = getName.StartsWith("You") ? "have" : "has";
                        currentActivity.Add(string.Format("{0} {3} an upcoming event today - {1} starting {2}", getName, label, starting, pronoun));
                    }
                    else
                    {
                        string pronoun = getName.StartsWith("You") ? "are" : "is";
                        currentActivity.Add(string.Format("{0} {3} currently at an event - {1} {2}", getName, label, finishing, pronoun));
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
