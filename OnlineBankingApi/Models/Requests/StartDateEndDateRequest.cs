using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [StartDateEndDateRequestValidation]
    public class StartDateEndDateRequest
    {
        public DateTime StartDate { set; get; }
        public DateTime EndDate { set; get; }
    }
}
