using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.MiFinance.Enums
{
    public enum Categories
    {
        Full = 21,
        Half,
        Missort,
        CWTL = 33,
        UberEats,
        CCInterest = 42,
        MissedEntries = 1140,
        Bills = 1144,
        Flex = 1147,
        SavingsPot, // of type income
        Savings, // of type spending,
        MonzoTransaction = 2151
    }
}
