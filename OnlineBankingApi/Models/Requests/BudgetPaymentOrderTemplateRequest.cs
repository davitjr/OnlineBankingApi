using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [BudgetPaymentOrderTemplateRequestValidation]
    public class BudgetOrderTemplateRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public XBS.BudgetPaymentOrderTemplate Template { set; get; }
    }
}
