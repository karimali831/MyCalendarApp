using Appology.DTOs;
using Appology.Enums;
using Appology.Model;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Appology.Helpers
{
    public static class Utils
    {
        public static bool IsLocal()
        {
            string host = HttpContext.Current.Request.Url.Host.ToLower();
            return (host == "localhost");
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static bool UnstrictCompare(this string str1, string str2)
        {
            return (RemoveSpecialCharacters(str1).Equals(RemoveSpecialCharacters(str2), StringComparison.InvariantCultureIgnoreCase));
        }

        public static string CleanEnumName<T>(T value) => Regex.Replace(value.ToString(), @"([a-z])([A-Z])", "$1 $2");

        public static string GenerateRandomString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            var sb = new StringBuilder();
            using (var provider = new RNGCryptoServiceProvider())
            {
                while (sb.Length != length)
                {
                    byte[] oneByte = new byte[1];
                    provider.GetBytes(oneByte);
                    char character = (char)oneByte[0];
                    if (valid.Contains(character))
                    {
                        sb.Append(character);
                    }
                }
            }

            return sb.ToString();
        }

        public static IHtmlString ResponseError(this ResourceWithError resource)
        {
            if (resource.NoErrors())
            {
                return null;
            }

            var error = JsonConvert.DeserializeObject<object>(HttpUtility.HtmlDecode(resource.Error));

            var formattedError = "<pre>" + JsonConvert.SerializeObject(error, Formatting.Indented) + "</pre>";

            return new HtmlString(string.Format(@"
            <div id='error_explanation' class='alert alert-danger'>
                {0}
                {1}
            </div>", ErrorCodeString(resource.ErrorCode.Value), error != null ? formattedError : string.Empty));
        }

        private static string ErrorCodeString(HttpStatusCode code)
        {
            var codeName = code.ToString();

            if ((int)code == 422)
            {
                codeName = "Unprocessable";
            }

            return "<h4>" + (int)code + @" - " + codeName + @"</h4>";
        }



        public static string ToCurrency(decimal amount) => amount.ToString("C", CultureInfo.CreateSpecificCulture("en-GB"));

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static string UppercaseFirst(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}
