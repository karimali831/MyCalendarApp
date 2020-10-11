using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCalendar.Helpers
{
    public static class LogHelper
    {
        public static void Log(string line)
        {
            using (StreamWriter w = File.AppendText(String.Format("{0}/Logs/info.txt", AppDomain.CurrentDomain.BaseDirectory)))
            {
                w.WriteLine(String.Format("{0} - {1}", DateTime.Now, line));
            }
        }
    }
}
