using Cronofy;
using MyCalendar.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace MyCalendar.Service
{
    public interface ICronofyService 
    {
        void SetToken(OAuthToken token);
        OAuthToken GetOAuthToken(string code);
        Account GetAccount();
        string GetAuthUrl();
        IEnumerable<Calendar> GetCalendars();
        IEnumerable<Profile> GetProfiles();
        IEnumerable<Event> ReadEventsForCalendar(string calendarId);
        IEnumerable<Event> ReadEvents();
        bool LoadUser(Model.User user);
        void UpsertEvent(string eventId, string calendarId, string summary, string description, DateTime start, DateTime end, string color, Location location = null);
        void DeleteEvent(string calendarId, string eventId);
    }

    public class CronofyService : ICronofyService
    {
        private static string _cronofyUid;
        private static string _accessToken;
        private static string _refreshToken;

        private static CronofyOAuthClient _oauthClient;

        public CronofyService()
        {
        }

        public bool LoadUser(Model.User user)
        {
            if (user == null)
            {
                LogHelper.Log(String.Format("LoadUser failed - Unable to find user - CronofyUID=`{0}`", user.CronofyUid));

                return false;
            }

            LogHelper.Log(String.Format("LoadUser success - CronofyUID=`{0}`", user.CronofyUid));

            
            _cronofyUid = user.CronofyUid;
            _accessToken = user.AccessToken;
            _refreshToken = user.RefreshToken;

            return true;
        }
        private CronofyOAuthClient OAuthClient
        {
            get
            {
                if (_oauthClient == null)
                    _oauthClient = new CronofyOAuthClient(ConfigurationManager.AppSettings["CronofyClientId"], ConfigurationManager.AppSettings["CronofyClientSecret"]);
                return _oauthClient;
            }
        }

        private CronofyAccountClient _accountClient;
        private CronofyAccountClient AccountClient
        {
            get
            {
                if (_accountClient == null)
                    _accountClient = new CronofyAccountClient(_accessToken);
                return _accountClient;
            }
        }


        private string _oauthCallbackUrl = string.Format("{0}/cronofy/auth", ConfigurationManager.AppSettings["RootUrl"]);
        private string _oauthAccountIdCallbackUrl = string.Format("{0}/availability/AccountId", ConfigurationManager.AppSettings["RootUrl"]);


        public void SetToken(OAuthToken token)
        {
            if (token != null)
            {
                _accessToken = token.AccessToken;
                _refreshToken = token.RefreshToken;

                _accountClient = new CronofyAccountClient(_accessToken);
            }
        }

        public string GetAuthUrl()
        {
            return OAuthClient.GetAuthorizationUrlBuilder(_oauthCallbackUrl).ToString();
        }

        public string GetAccountIdAuthUrl()
        {
            return OAuthClient.GetAuthorizationUrlBuilder(_oauthAccountIdCallbackUrl).ToString();
        }

        public Account GetAccount()
        {
            Account account = null;

            try
            {
                account = CronofyAccountRequest(() => { return AccountClient.GetAccount(); });
                LogHelper.Log("GetAccount success");
            }
            catch (CronofyException ex)
            {
                LogHelper.Log(string.Format("GetAccount failure - {0}", ex.Message));
            }

            return account;
        }

        public OAuthToken GetOAuthToken(string code)
        {
            OAuthToken token = null;

            try
            {
                token = OAuthClient.GetTokenFromCode(code, _oauthCallbackUrl);
                LogHelper.Log(string.Format("GetOAuthToken success - code=`{0}`", code));
            }
            catch (CronofyException ex)
            {
                LogHelper.Log(string.Format("GetOAuthToken failure - code=`{0}` - {1}", code, ex.Message));
            }

            return token;
        }

        public OAuthToken GetAccountIdOAuthToken(string code)
        {
            OAuthToken token = null;

            try
            {
                token = OAuthClient.GetTokenFromCode(code, _oauthAccountIdCallbackUrl);
                LogHelper.Log(string.Format("GetAccountIdOAuthToken success - code=`{0}`", code));
            }
            catch (CronofyException)
            {
                LogHelper.Log(string.Format("GetAccountIdOAuthToken failure - code=`{0}`", code));
            }

            return token;
        }


        public IEnumerable<Profile> GetProfiles()
        {
            IEnumerable<Profile> profiles = new Profile[0];

            try
            {
                profiles = CronofyAccountRequest(() => { return AccountClient.GetProfiles(); });
                LogHelper.Log("GetProfiles success");
            }
            catch (CronofyException ex)
            {
                LogHelper.Log(string.Format("GetProfiles failure - {0}", ex.Message));
            }

            return profiles;
        }

        public IEnumerable<Calendar> GetCalendars()
        {
            IEnumerable<Calendar> calendars = new Calendar[0];

            try
            {
                calendars = CronofyAccountRequest(() => { return AccountClient.GetCalendars(); });
                LogHelper.Log("GetCalendars success");
            }
            catch (CronofyException ex)
            {
                LogHelper.Log(string.Format("GetCalendars failure - {0}", ex.Message));
            }

            return calendars;
        }

        public Calendar CreateCalendar(string profileId, string name)
        {
            Calendar calendar = null;

            try
            {
                calendar = CronofyAccountRequest(() => { return AccountClient.CreateCalendar(profileId, name); });
                LogHelper.Log(string.Format("CreateCalendar success - profileId=`{0}` - name=`{1}`", profileId, name));
            }
            catch (CronofyException ex)
            {
                LogHelper.Log(string.Format("CreateCalendar failure - profileId=`{0}` - name=`{1}` - {2}", profileId, name, ex.Message));
                throw;
            }

            return calendar;
        }

        public IEnumerable<FreeBusy> GetFreeBusy()
        {
            IEnumerable<FreeBusy> freeBusy = new FreeBusy[0];

            try
            {
                freeBusy = CronofyAccountRequest(() => { return AccountClient.GetFreeBusy(); });
                LogHelper.Log("GetFreeBusy success");
            }
            catch (CronofyException)
            {
                LogHelper.Log("GetFreeBusy failure");
            }

            return freeBusy;
        }

        public IEnumerable<Event> ReadEventsForCalendar(string calendarId)
        {
            IEnumerable<Event> events = new Event[0];

            var readEvents = new GetEventsRequestBuilder()
                .IncludeManaged(true)
                .CalendarId(calendarId)
                .Build();

            try
            {
                events = CronofyAccountRequest(() => { return AccountClient.GetEvents(readEvents); });
                LogHelper.Log(string.Format("ReadEventsForCalendar success - calendarId=`{0}`", calendarId));
            }
            catch (CronofyException ex)
            {
                LogHelper.Log(string.Format("ReadEventsForCalendar failure - calendarId=`{0}` - {1}", calendarId, ex.Message));
            }

            return events;
        }

        public IEnumerable<Event> ReadEvents()
        {
            IEnumerable<Event> events = new Event[0];

            var readEvents = new GetEventsRequestBuilder()
                .IncludeManaged(true)
                .Build();

            try
            {
                events = CronofyAccountRequest(() => { return AccountClient.GetEvents(readEvents); });
                LogHelper.Log("ReadEvents success");
            }
            catch (CronofyException ex)
            {
                LogHelper.Log(string.Format("ReadEvents failure - {0}", ex.Message));
            }

            return events;
        }

        public void UpsertEvent(string eventId, string calendarId, string summary, string description, DateTime start, DateTime end, string color, Location location = null)
        {
            int[] reminders = new int[] { 30,1440, 0  };


            var buildingEvent = new UpsertEventRequestBuilder()
                .EventId(eventId)
                .Summary(summary)
                .Description(description)
                .Start(start)
                .End(end)
                .Reminders(reminders)
                .Color(color)
                .StartTimeZoneId("Europe/London")
                .EndTimeZoneId("Europe/London")
                .TimeZoneId("Europe/London");

            if (location != null)
            {
                buildingEvent.Location(location.Description,
                                       string.IsNullOrEmpty(location.Latitude) ? null : location.Latitude,
                                       string.IsNullOrEmpty(location.Longitude) ? null : location.Longitude);
            }

            var builtEvent = buildingEvent.Build();

            try
            {
                CronofyAccountRequest(() => { AccountClient.UpsertEvent(calendarId, builtEvent); });

                var successLog = "UpsertEvent success - eventId=`{eventId}` - calendarId=`{calendarId}` - summary=`{summary}` - description=`{description}` - start=`{start}` - end=`{end}`";

                if (location != null && !(string.IsNullOrEmpty(location.Latitude) || string.IsNullOrEmpty(location.Longitude)))
                    successLog += string.Format(" - location.lat=`{0}` - location.long=`{1}`", location.Latitude, location.Longitude);

                LogHelper.Log(successLog);
            }
            catch (CronofyException)
            {
                var failureLog = "UpsertEvent failure - eventId=`{eventId}` - calendarId=`{calendarId}` - summary=`{summary}` - description=`{description}` - start=`{start}` - end=`{end}`";

                if (location != null && !(string.IsNullOrEmpty(location.Latitude) || string.IsNullOrEmpty(location.Longitude)))
                    failureLog += string.Format(" - location.lat=`{0}` - location.long=`{1}`", location.Latitude, location.Longitude);

                LogHelper.Log(failureLog);
                throw;
            }
        }

        public void DeleteEvent(string calendarId, string eventId)
        {
            try
            {
                CronofyAccountRequest(() => { AccountClient.DeleteEvent(calendarId, eventId); });
                LogHelper.Log(string.Format("DeleteEvent success - calendarId=`{0}` - eventId=`{1}`", calendarId, eventId));
            }
            catch (CronofyException ex)
            {
                LogHelper.Log(string.Format("DeleteEvent failure - calendarId=`{0}` - eventId=`{1}` - {2}", calendarId, eventId, ex.Message));
            }
        }

        public void DeleteExtEvent(string calendarId, string EventUid)
        {
            try
            {
                CronofyAccountRequest(() => { AccountClient.DeleteExternalEvent(calendarId, EventUid); });
                LogHelper.Log(string.Format("DeleteExtEvent success - calendarId=`{0}` - eventId=`{1}`", calendarId, EventUid));
            }
            catch (CronofyException ex)
            {
                LogHelper.Log(string.Format("DeleteExtEvent failure - calendarId=`{0}` - eventId=`{1}` - {2}", calendarId, EventUid, ex.Message));
            }
        }


        public IEnumerable<Channel> GetChannels()
        {
            IEnumerable<Channel> channels = new Channel[0];

            try
            {
                channels = CronofyAccountRequest(() => { return AccountClient.GetChannels(); });
                LogHelper.Log("GetChannels success");
            }
            catch (CronofyException ex)
            {
                LogHelper.Log(string.Format("GetChannels failure - {0}", ex.Message));
            }

            return channels;
        }

        public Channel CreateChannel(string path, bool onlyManaged, IEnumerable<string> calendarIds)
        {
            Channel channel = null;

            var builtChannel = new CreateChannelBuilder()
                .CallbackUrl(path)
                .OnlyManaged(onlyManaged)
                .CalendarIds(calendarIds)
                .Build();

            try
            {
                channel = CronofyAccountRequest(() => { return AccountClient.CreateChannel(builtChannel); });
                LogHelper.Log(string.Format("CreateChannel success - path=`{0}` - onlyManaged=`{1}` - calendarIds=`{2}`", path, onlyManaged, string.Join(",", calendarIds)));
            }
            catch (CronofyException)
            {
                LogHelper.Log(string.Format("CreateChannel failure - path=`{0}` - onlyManaged=`{1}` - calendarIds=`{2}`", path, onlyManaged, string.Join(",", calendarIds)));
                throw;
            }

            return channel;
        }

        public void CronofyAccountRequest(Action request)
        {
            CronofyAccountRequest(() => { request(); return true; });
        }

        public T CronofyAccountRequest<T>(Func<T> request)
        {
            T response = default(T);

            try
            {
                // This may fail due to an out of day access token
                response = request();
            }
            catch (CronofyException ex1)
            {
                try
                {
                    // First time this fails, attempt to get a new access token and store it for the user
                    var token = OAuthClient.GetTokenFromRefreshToken(_refreshToken);

                    //userRepository.CronofyAccountRequest(token.AccessToken, token.RefreshToken, _cronofyUid); 

                    SetToken(token);

                    LogHelper.Log(string.Format("Access Token out of date, tokens have been refreshed - _cronofyUid=`{0}` - {1}", _cronofyUid, ex1.Message));
                }
                catch (CronofyException ex2)
                {
                    LogHelper.Log(string.Format("Credentials invalid, deleting account - _cronofyUid=`{0}` - {1} - {2}", _cronofyUid, ex1.Message, ex2.Message));

                    throw new CredentialsInvalidError();
                }

                // Second time we perform the request, if it fails we want it to propagate up to the method that
                // called CronofyAccountRequest to log the appropriate information
                response = request();
            }

            return response;
        }


    }

    public class CredentialsInvalidError : Exception
    {
        public CredentialsInvalidError()
        {
        }
    }
}
