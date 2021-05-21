using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [ReceivedFastTransferPaymentOrderRequestValidation]
    public class ReceivedFastTransferPaymentOrderRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public XBS.ReceivedFastTransferPaymentOrder Order { get; set; }
    }
}
