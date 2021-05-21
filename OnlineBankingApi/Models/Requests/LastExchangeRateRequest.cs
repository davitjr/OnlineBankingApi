using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [LastExchangeRateRequestValidation]
    public class LastExchangeRateRequest
    {
        [Required(ErrorMessage = "Արժույթն ընտրված չէ։")]
        public string Currency { set; get; }

        [Required(ErrorMessage = "Փոխարժեքի տեսակն ընտրված չէ։")]
        public byte RateType { set; get; }

        [Required(ErrorMessage = "Փոխարկման տեսակն ընտրված չէ։")]
        public byte Direction { set; get; }
    }
}
