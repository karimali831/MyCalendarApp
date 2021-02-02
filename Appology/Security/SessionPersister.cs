using Appology.Helpers;
using Ninject.Activation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Appology.Security
{
    public static class SessionPersister
    {
        static string emailSessionVar = ConfigurationManager.AppSettings["AuthenticationName"];

        public static string EmailOrig
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

        public static string Email
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    return null;
                }
                
                var cookie = HttpContext.Current.Request.Cookies.Get(emailSessionVar);
                if (cookie != null && cookie.Value != string.Empty)
                {
                   return cookie.Value;
                }

                return null;
            }
            set
            {
                HttpCookie cookie = new HttpCookie(emailSessionVar)
                {
                    Value = value,
                    Expires = DateUtils.DateTime().AddMinutes(43800)
                };

                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }
    }
}