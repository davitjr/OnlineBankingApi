using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CustomerNumberRequestValidation]
    public class CustomerNumberRequest
    {
        public ulong CustomerNumber { get; set; }
    }
}
