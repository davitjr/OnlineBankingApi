using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [PeriodicTransferHistoryRequestValidation]
    public class PeriodicTransferHistoryRequest
    {
        public long ProductId { set; get; }
        public DateTime DateFrom { set; get; }
        public DateTime DateTo { set; get; }
    }
}
