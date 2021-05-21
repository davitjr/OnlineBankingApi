using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [PeriodicBudgetPaymentOrderRequestValidation]
    public class PeriodicBudgetPaymentOrderRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public XBS.PeriodicBudgetPaymentOrder Order { set; get; }
    }
}
