using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [LoanContractRequestValidation]
    public class LoanContractRequest
    {
        public long DocId { set; get; }
        [Range(1,2,ErrorMessage = "Տեսակն ընտրված չէ։")]
        public int ProductType { set; get; }
        public bool FromApprove { set; get; }
    }
}
