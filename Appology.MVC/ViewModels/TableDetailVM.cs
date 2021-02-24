using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;

namespace Appology.Website.ViewModels
{
    public class TableDetailVM
    {
        public IList<string> TableColumns { get; set; }
        public IList<Func<object, IHtmlString>> RowsContent { get; set; }
    }
}