using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [AttachedCardTariffsRequestValidation]
    public class AttachedCardTariffsRequest
    {
        [Required(ErrorMessage = "Հիմնական քարտի համարը լրացված չէ։")]
        public string MainCardNumber { get; set; }
        public uint cardType { get; set; }
    }
}
