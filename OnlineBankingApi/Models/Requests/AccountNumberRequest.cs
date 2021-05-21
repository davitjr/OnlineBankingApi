using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [AccountNumberRequestValidation]
    public class AccountNumberRequest
    {
        [Required(ErrorMessage = "Հաշվեհամարը դաշտը բացակայում է։")]
        public string AccountNumber { get; set; }
    }
}
