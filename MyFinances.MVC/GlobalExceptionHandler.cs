using DFM.ExceptionHandling;
using System;

namespace MyFinances.Website.App_Start
{
    public static class GlobalExceptionHandler
    {
        public static Func<Exception, IErrorBuilder> ReportException = (ex) =>
        {
            throw new InvalidOperationException("Global Exception handler has not been initialised", ex);
        };
    }
}