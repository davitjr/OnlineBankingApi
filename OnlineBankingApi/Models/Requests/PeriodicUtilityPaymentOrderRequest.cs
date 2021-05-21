using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [UtilityperiodicPaymentRequestValidation]   
    public class PeriodicUtilityPaymentOrderRequest
    {
        public XBS.PeriodicUtilityPaymentOrder Order { set; get; }
    }
}
