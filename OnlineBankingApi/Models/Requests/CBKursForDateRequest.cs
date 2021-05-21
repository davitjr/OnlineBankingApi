using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CBKursForDateRequestValidation]
    public class CBKursForDateRequest
    {
        [Required(ErrorMessage = "Արժույթ դաշտը լրացված չէ։")]
        public string Currency { set; get; }
        public DateTime Date { set; get; }
    }
}
