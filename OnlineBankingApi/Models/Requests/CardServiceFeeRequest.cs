using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CardServiceFeeRequestValidation]
    public class CardServiceFeeRequest
    {
        public int CardType { set; get; }
        public int OfficeId { set; get; }

        [Required(ErrorMessage = "Արժույթ դաշտը պարտադիր է։")]
        public string Currency { set; get; }
    }
}
