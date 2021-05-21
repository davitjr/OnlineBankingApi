using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using OnlineBankingApi.Filters;

namespace OnlineBankingApi.Models.Requests
{
    [DepositRepaymentsPriorRequestValidation]
    public class DepositRepaymentsPriorRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public XBS.DepositRepaymentRequest Request { get; set; }
    }
}
