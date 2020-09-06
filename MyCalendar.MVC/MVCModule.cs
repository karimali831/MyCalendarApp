using Ninject.Modules;
using MyCalendar.Repository;
using MyCalendar.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCalendar.Ninject
{
    public class MVCModule : NinjectModule
    {
        public override void Load()
        {
            // Services
            Bind<ICalendarService>().To<CalendarService>();

            // Repositories
            Bind<ICalendarRepository>().To<CalendarRepository>();
        }
    }


}