using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MyCalendar.Security
{
    public static class SessionPersister
    {
        static string emailSessionVar = ConfigurationManager.AppSettings["AuthenticationName"];

        public static string Email
        {
            get
            {
                if (HttpContext.Current == null)
                    return string.Empty;
                var sessionVar = HttpContext.Current.Session[emailSessionVar];
                if (sessionVar != null)
                    return sessionVar as string;
                return null;
            }
            set
            {
                HttpContext.Current.Session[emailSessionVar] = value;
            }
        }
    }
}