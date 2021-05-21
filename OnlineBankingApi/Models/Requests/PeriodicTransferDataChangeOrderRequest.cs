using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [PeriodicTransferDataChangeOrderRequestValidation]
    public class PeriodicTransferDataChangeOrderRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public XBS.PeriodicTransferDataChangeOrder Order { set; get; }
    }
}
