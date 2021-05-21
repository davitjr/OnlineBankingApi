using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [RemovalOrderRequestValidation]
    public class RemovalOrderRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public XBS.RemovalOrder Order { set; get; }
        public string OTP { get; set; }
        public string OTPForSecondConfirmation { get; set; }
    }
}
