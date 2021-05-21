using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{

    public class PlasticCardSMSServiceOrderRequest
    {
        public XBS.PlasticCardSMSServiceOrder Order { set; get; }
    }
}
