using Appology.Security;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Appology.Helpers
{
    public static class LogHelper
    {
        public static async Task Log(string line)
        {
            using (StreamWriter w = File.AppendText(String.Format("{0}/Logs/info.txt", AppDomain.CurrentDomain.BaseDirectory)))
            {
                await w.WriteLineAsync(string.Format("{0} - {1}", DateTime.Now, line));
            }
        }

        public static void LogDapperQuery(string type, string method, string sqlTxt, bool status)
        {
            string queryLogDir = $"{AppDomain.CurrentDomain.BaseDirectory}/Logs/DapperQueries/{(status ? "" : "Failed/")}";
            string fileName = string.Format("{0}{1}-{2}.txt", queryLogDir, type, method);
            string contents = string.Format("{0} - {1} {2}", DateTime.Now, SessionPersister.Email ?? "(no user)", sqlTxt);

            File.WriteAllText(fileName, contents);
        }
    }
}
