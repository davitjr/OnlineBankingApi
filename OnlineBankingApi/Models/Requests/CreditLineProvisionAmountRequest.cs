using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CreditLineProvisionAmountRequestValidation]
    public class CreditLineProvisionAmountRequest
    {
        
        public double Amount { set; get; }

        [Required(ErrorMessage = "Վարկի արժույթն ընտրված չէ։")]
        public string LoanCurrency { set; get; }

        [Required(ErrorMessage = "Գրավադրված գումարի արժույթն ընտրված չէ։")]
        public string ProvisionCurrency { set; get; }

        public bool MandatoryPayment { set; get; }
        public int CreditLineType { set; get; }
    }
}
