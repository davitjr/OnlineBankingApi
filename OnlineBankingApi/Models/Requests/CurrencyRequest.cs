using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CurrencyRequestValidation]
    public class CurrencyRequest
    {
        [Required(ErrorMessage = "Արժույթն ընտրված չէ։")]
        public string Currency { get; set; }
    }
}
