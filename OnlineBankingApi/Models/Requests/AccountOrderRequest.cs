using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBS;

namespace OnlineBankingApi.Models.Requests
{
    [AccountOrderRequestValidation]
    public class AccountOrderRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public AccountOrder Order { set; get; }
    }
}
