using OnlineBankingApi.Filters;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [BudgetPaymentOrderRequestValidation]
    public class BudgetPaymentOrderRequest
    {
        public XBS.BudgetPaymentOrder Order { set; get; }

        
    }
}
