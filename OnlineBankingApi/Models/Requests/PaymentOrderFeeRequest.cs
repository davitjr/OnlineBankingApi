using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [PaymentOrderFeeRequestValidation]
    public class PaymentOrderFeeRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public XBS.PaymentOrder Order { set; get; }
    }
}

