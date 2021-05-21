using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class ManuallyRateChangingAccessRequest
    {
        public double Amount { get; set; }
        [Required(ErrorMessage = "Արժույթ դաշտը պարտադիր է։")]
        public string Currency { get; set; }
    }
}
