using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [InternationalPaymentOrderRequestValidation]
    public class InternationalPaymentOrderRequest
    {
        [Required(ErrorMessage ="Հարցման պարունակությունը դատարկ է։")]
        public XBS.InternationalPaymentOrder Order { set; get; }
    }

    [InternationalPaymentOrderFeeRequestValidation]
    public class InternationalPaymentOrderFeeRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public XBS.InternationalPaymentOrder Order { set; get; }
    }
}
