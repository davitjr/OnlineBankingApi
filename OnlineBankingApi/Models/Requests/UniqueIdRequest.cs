using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [UniqueIdRequestValidation]
    public class UniqueIdRequest
    {
        public double UniqueId { get; set; }
    }
}
