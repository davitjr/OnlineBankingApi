using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [ApproveLoanProductOrderRequestValidation]
    public class ApproveLoanProductOrderRequest
    {
        public long Id { set; get; }
        public short ProductType { set; get; }
        public string OTP { get; set; }
        public string OTPForSeconfConfirmation { get; set; }
    }
}
