using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Configuration;

namespace Appology.MiCalendar.Helpers
{
    public static class CalendarUtils
    {

        public static string AvatarSrc(Guid userId, string avatar, string name, bool absoluteUrl = false)
        {
            string avatarSrc;

            if (!string.IsNullOrEmpty(avatar))
            {
                if (avatar.StartsWith("0"))
                {
                    avatarSrc = $"/Content/img/avatar/pk1/{Path.GetFileName(avatar)}";
                }
                else
                {
                    avatarSrc = $"/Content/img/avatar/user/{userId.ToString().ToLower()}/{avatar}";
                }

                avatarSrc = $"{(absoluteUrl ? ConfigurationManager.AppSettings["RootUrl"] : "")}{avatarSrc}";
            }
            else
            {
                avatarSrc = name.ToUpper().Substring(0, 2);
            }


            return avatarSrc;
        }

        public static Color GetSystemDrawingColorFromHexString(string hexString)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(hexString, @"[#]([0-9]|[a-f]|[A-F]){6}\b"))
                throw new ArgumentException();
            int red = int.Parse(hexString.Substring(1, 2), NumberStyles.HexNumber);
            int green = int.Parse(hexString.Substring(3, 2), NumberStyles.HexNumber);
            int blue = int.Parse(hexString.Substring(5, 2), NumberStyles.HexNumber);
            return Color.FromArgb(red, green, blue);
        }

        public static string ContrastColor(string color)
        {
            // Convert
            var iColor = GetSystemDrawingColorFromHexString(color);

            // Calculate the perceptive luminance (aka luma) - human eye favors green color... 
            double luma = ((0.299 * iColor.R) + (0.587 * iColor.G) + (0.114 * iColor.B)) / 255;

            // Return black for bright colors, white for dark colors
            return luma > 0.5 ? Color.Black.Name : Color.White.Name;
        }

    }
}