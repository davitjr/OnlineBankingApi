using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CheckDepositOrderConditionRequestValidation]
    public class CheckDepositOrderConditionRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public XBS.DepositOrder Order{ get; set; }
    }
}
