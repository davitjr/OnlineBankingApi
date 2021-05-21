using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [AmountRequestValidation]
    public class AmountRequest
    {
        public double Amount { get; set; }
    }
}
