using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.MiFinance.Enums
{
    public enum PaymentStatus
    {
        Ended,
        Paid,
        Upcoming,
        Late,
        Unknown,
        DueToday
    }
}
