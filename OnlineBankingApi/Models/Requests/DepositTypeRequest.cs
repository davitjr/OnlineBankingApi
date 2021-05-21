using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [DepositTypeRequestValidation]
    public class DepositTypeRequest
    {
        [Required(ErrorMessage = "Ավանդի տեսակը նշված չէ։")]
        public short DepositType { get; set; }
    }
}
