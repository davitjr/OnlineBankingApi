using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CardNumberRequestValidation]
    public class CardNumberRequest
    {
        [Required(ErrorMessage = "Քարտի համարը բացակայում է։")]
        public string CardNumber { get; set; }
    }
}
