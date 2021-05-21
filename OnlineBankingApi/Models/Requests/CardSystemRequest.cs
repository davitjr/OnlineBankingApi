using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CardSystemRequestValidation]
    public class CardSystemRequest
    {
        public int CardSystem { get; set; }
    }
}
