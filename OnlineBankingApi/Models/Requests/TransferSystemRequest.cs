using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [TransferSystemRequestValidation]
    public class TransferSystemRequest
    {
        public int TransferSystem { set; get; }
    }
}
