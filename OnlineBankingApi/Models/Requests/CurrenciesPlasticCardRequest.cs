using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CurrenciesPlasticCardRequestValidation]
    public class CurrenciesPlasticCardRequest
    {
        public ushort CardType { get; set; }
        public short PeriodicityType { get; set; }
    }
}
