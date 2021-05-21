using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [LoanOrCreditLineContractRequestValidation]
    public class LoanOrCreditLineContractRequest
    {
        [Required(ErrorMessage = "Վարկի ունիկալ համարը լրացված չէ։")]
        public string LoanNumber { set; get; }

        public byte Type { set; get; }
    }
}
