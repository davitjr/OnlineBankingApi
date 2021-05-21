using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBS;

namespace OnlineBankingApi.Models.Requests
{
    [AccountClosingOrderRequestValidation]
    public class AccountClosingOrderRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public AccountClosingOrder Order { set; get; }
    }
}
