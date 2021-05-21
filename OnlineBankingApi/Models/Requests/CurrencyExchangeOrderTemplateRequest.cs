using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [CurrencyExchangeOrderTemplateRequestValidation]
    public class CurrencyExchangeOrderTemplateRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public XBS.CurrencyExchangeOrderTemplate Template { set; get; }
    }
}
