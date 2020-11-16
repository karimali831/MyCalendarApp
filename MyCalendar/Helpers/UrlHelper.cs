using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyCalendar.Helpers
{
    public static class UrlAssist
    {
        public static string ResolveServerUrl(string serverUrl, bool forceHttps)
        {
            // No HTTP context? Just return original URL
            if (HttpContext.Current == null)
                return serverUrl;

            // *** Is it already an absolute Url?
            if (serverUrl.IndexOf("://") > -1)
                return serverUrl;

            // *** Start by fixing up the Url an Application relative Url
            string newUrl = ResolveUrl(serverUrl);

            Uri originalUri = HttpContext.Current.Request.Url;
            newUrl = (forceHttps ? "https" : originalUri.Scheme) +
                     "://" + originalUri.Authority + newUrl;

            return newUrl;
        }

        public static string ResolveUrl(string originalUrl)
        {
            if (originalUrl == null)
                return null;

            // *** Absolute path - just return
            if (originalUrl.IndexOf("://") != -1)
                return originalUrl;

            // *** Fix up image path for ~ root app dir directory
            if (originalUrl.StartsWith("~"))
            {
                string newUrl = "";
                if (HttpContext.Current != null)
                    newUrl = HttpContext.Current.Request.ApplicationPath +
                          originalUrl.Substring(1).Replace("//", "/");
                else
                    // *** Not context: assume current directory is the base directory
                    throw new ArgumentException("Invalid URL: Relative URL not allowed.");

                // *** Just to be sure fix up any double slashes
                return newUrl.Replace("//", "/");
            }

            return originalUrl;
        }

        public static string ResolveServerUrl(string serverUrl)
        {
            return ResolveServerUrl(serverUrl, false);
        }
    }
}
