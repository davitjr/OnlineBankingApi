using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBS;

namespace OnlineBankingApi.Models.Requests
{
    
    public class OrderRejectionRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public OrderRejection OrderRejection { get; set; }

        public string OTP { get; set; }
    }
}
