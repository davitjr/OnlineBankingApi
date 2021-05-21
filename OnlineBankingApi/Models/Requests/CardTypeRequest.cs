using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CardTypeRequestValidation]
    public class CardTypeRequest
    {
        [Required(ErrorMessage = "Քարտի տեսակը պարտադիր է։")]
        [Range(1, 100, ErrorMessage = "Քարտի տեսակը սխալ է։")]
        public ushort CardType { set; get; }
    }
}
