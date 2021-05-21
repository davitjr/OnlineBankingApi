using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CreditLineLastDateRequestValidation]
    public class CreditLineLastDateRequest
    {
        public int CreditLineType { set; get; }

        [Required(ErrorMessage = "Քարտի համարը լրացված չէ։")]
        public string CardNumber { set; get; }
    }
}
