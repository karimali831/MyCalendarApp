using Dapper;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Appology.Helpers;
using System.Reflection;
using System.Diagnostics;
using DFM.ExceptionHandling.Sentry;
using DFM.ExceptionHandling;
using System.Configuration;
using System.Web;

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
            this.exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
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
                exceptionHandlerService.ReportException(exp).Submit();

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
                exceptionHandlerService.ReportException(exp).Submit();

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
                exceptionHandlerService.ReportException(exp).Submit();
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
                exceptionHandlerService.ReportException(exp).Submit();
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
                exceptionHandlerService.ReportException(exp).Submit();
                return default;
            }
        }

        private void LogDapperQuery(string query, bool status)
        {
            if (HttpContext.Current.Request.IsLocal || !status)
            {
                StackTrace stackTrace = new StackTrace();
                var frame = stackTrace.GetFrame(6);

                if (frame != null)
                {
                    string method = frame.GetMethod().Name;
                    string type = frame.GetMethod().ReflectedType.Name;
                    LogHelper.LogDapperQuery(type, method, query, status);
                }
            }
        }
    }
}
