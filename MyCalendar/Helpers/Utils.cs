using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFinances.Helpers
{
    public static class Utils
    {
        public static string Duration(DateTime d1, DateTime d2)
        {
            string rtnString = "";
            TimeSpan span = (d1 - d2);

            if (span.Minutes < 60 && span.Hours == 0 && span.Days == 0)
            {
                rtnString = $"{span.Minutes} minute{(span.Minutes > 1 ? "s" : "")}";
            }
            else if (span.Hours >= 1 && span.Days == 0)
            {
                rtnString = $"{span.Hours} hour{(span.Hours > 1 ? "s" : "")}";

                if (span.Minutes > 0)
                {
                    rtnString += $", {span.Minutes} minute{(span.Minutes > 1 ? "s" : "")}";
                }
            }
            else if (span.Days >= 1)
            {
                rtnString = $"{span.Days} day{(span.Days > 1 ? "s" : "")}";

                if (span.Hours > 1)
                {
                    rtnString += $", {span.Hours} hour{(span.Hours > 1 ? "s" : "")}";
                }

                if (span.Minutes > 0)
                {
                    rtnString += $", {span.Minutes} minute{(span.Minutes > 1 ? "s" : "")}";
                }
            }

            return rtnString;
        }
    }
}
