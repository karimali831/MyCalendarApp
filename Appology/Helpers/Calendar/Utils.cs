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

namespace Appology.MiCalendar.Helpers
{
    public static class CalendarUtils
    {

        public static string AvatarSrc(Guid userId, string avatar, string name)
        {
            if (!string.IsNullOrEmpty(avatar))
            {
                if (avatar.StartsWith("0"))
                {
                    return $"/Content/img/avatar/pk1/{Path.GetFileName(avatar)}";
                }
                else
                {
                    return $"/Content/img/avatar/user/{userId.ToString().ToLower()}/{avatar}";
                }
            }
            else
            {
                return name.ToUpper().Substring(0, 2);
            }
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