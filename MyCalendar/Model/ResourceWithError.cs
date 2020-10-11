using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cronofy;

namespace MyCalendar.Model
{
    public abstract class ResourceWithError
    {
        public HttpStatusCode? ErrorCode { get; set; }
        public string Error { get; set; }

        public void SetError(CronofyResponseException ex)
        {
            ErrorCode = (HttpStatusCode)ex.Response.Code;
            Error = ex.Response.Body;
        }

        public bool NoErrors()
        {
            return !ErrorCode.HasValue;
        }
    }
}
