using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [LoanStatementRequestValidation]
    public class LoanStatementRequest
    {
       public ulong ProductId { set; get; }
       public DateTime DateFrom { set; get; }
       public DateTime DateTo { set; get; }
       public double MinAmount { set; get; }
       public double MaxAmount { set; get; }
       public string DebCred { set; get; }
       public int TransactionsCount { set; get; }
       public short OrderByAscDesc { set; get; }
    }
}
