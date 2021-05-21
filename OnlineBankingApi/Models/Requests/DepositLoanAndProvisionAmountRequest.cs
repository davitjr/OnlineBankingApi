using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class DepositLoanAndProvisionAmountRequest
    {
        public double Amount { set; get; }

        [Required(ErrorMessage = "Վարկի արժույթն ընտրված չէ։")]
        public string LoanCurrency { set; get; }

        [Required(ErrorMessage = "Գրավադրված գումարի արժույթն ընտրված չէ։")]
        public string ProvisionCurrency { set; get; }
    }
}
