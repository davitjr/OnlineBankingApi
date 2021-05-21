using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [PrintContractRequestValidation]
    public class PrintContractRequest
    {
        public long Id { set; get; }
    }
}
