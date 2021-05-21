using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class CrossConvertationVariantRequest
    {
        public string DebitCurrency { get; set; }
        public string CreditCurrency { get; set; }
    }
}
