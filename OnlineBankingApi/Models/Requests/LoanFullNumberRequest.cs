using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [LoanFullNumberRequestValidation]
    public class LoanFullNumberRequest
    {
        [Required(ErrorMessage = "Վարկային հաշիվը նշված չէ։")]
        public string LoanFullNumber { get; set; }
    }
}
