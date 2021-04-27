using Dapper;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Appology.Helpers;
using System.Diagnostics;
using DFM.ExceptionHandling.Sentry;
using DFM.ExceptionHandling;
using System.Configuration;
using System.Web;
using Appology.Security;

namespace Appology.Repository
{
    public interface IDapperBaseRepository
    {
        Task<List<T>> QueryAsync<T>(string query, object parameters = null, bool log = true);
        Task<bool> ExecuteAsync(string query, object parameters = null, bool log = true);
        Task<T> ExecuteScalarAsync<T>(string query, object parameters = null, bool log = true);
        Task<T> QueryFirstOrDefaultAsync<T>(string query, object parameters = null, bool log = true);
    }

    public class DapperBaseRepository : IDapperBaseRepository
    {
        private readonly IExceptionHandlerService exceptionHandlerService;
        private readonly Func<IDbConnection> dbConnectionFactory;

        public DapperBaseRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
            exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
        }

        public async Task<List<T>> QueryAsync<T>(string query, object parameters = null, bool log = true)
        {
            try
            {
                if (log)
                {
                    LogDapperQuery(query, true);
                }

                using (var sql = dbConnectionFactory())
                {
                    return (await sql.QueryAsync<T>(query, parameters)).ToList();
                }
            }
            catch (Exception exp)
            {
                LogDapperQuery(query, false);
                HandleQueryException(query, exp);
                return new List<T>();
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string query, object parameters = null, bool log = true)
        {
            try
            {
                using (var sql = dbConnectionFactory())
                {
                    if (log)
                    {
                        LogDapperQuery(query, true);
                    }

                    return (await sql.QueryFirstOrDefaultAsync<T>(query, parameters));
                }
            }
            catch (Exception exp)
            {
                LogDapperQuery(query, false);
                HandleQueryException(query, exp);
                return default; 
            }
        }

        public async Task<bool> ExecuteAsync(string query, object parameters = null, bool log = true)
        {
            try
            {
                using (var sql = dbConnectionFactory())
                {
                    if (log)
                    {
                        LogDapperQuery(query, true);
                    }

                    await sql.ExecuteAsync(query, parameters);
                    return true;
                }
            }
            catch (Exception exp)
            {
                LogDapperQuery(query, false);
                HandleQueryException(query, exp);
                return false;
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string query, object parameters = null, bool log = true)
        {
            try
            {
                using (var sql = dbConnectionFactory())
                {
                    if (log)
                    {
                        LogDapperQuery(query, true);
                    }

                    return await sql.ExecuteScalarAsync<T>(query, parameters);
                }
            }
            catch (Exception exp)
            {
                LogDapperQuery(query, false);
                HandleQueryException(query, exp);
                return default;
            }
        }

        public async Task<T> QuerySingleAsync<T>(string query, object parameters = null, bool log = true)
        {
            try
            {
                using (var sql = dbConnectionFactory())
                {
                    if (log)
                    {
                        LogDapperQuery(query, true);
                    }

                    return await sql.QuerySingleAsync<T>(query, parameters);
                }
            }
            catch (Exception exp)
            {
                LogDapperQuery(query, false);
                HandleQueryException(query, exp);
                return default;
            }
        }

        private void LogDapperQuery(string query, bool status)
        {
            if (HttpContext.Current.Request.IsLocal)
            {
                (string Type, string Method) Name = GetQueryName();
                LogHelper.LogDapperQuery(Name.Type, Name.Method, query, status);
            }
        }

        private (string Type, string Method) GetQueryName()
        {
            (string Type, string Method) Name = ("", "");

            StackTrace stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(7);

            if (frame != null)
            {
                string method = frame.GetMethod().Name;
                string type = frame.GetMethod().ReflectedType.Name;

                Name = (type, method);
            }

            return Name;
        }

        private void HandleQueryException(string sqlTxt, Exception exp)
        {
            (string Type, string Method) Name = GetQueryName();
            exp.Source += string.Format("{0} - {1} {3} {4} {2}", DateTime.Now, SessionPersister.Email ?? "(no user)", sqlTxt, Name.Type, Name.Method);

            // report to sentry
            exceptionHandlerService.ReportException(exp).Submit();
        }
    }
}
