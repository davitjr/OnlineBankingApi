using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CommisionAmountRequestValidation]
    public class CommisionAmountRequest
    {
        public double StartCapital { set; get; }
        public DateTime DateOfBeginning { set; get; }
        public DateTime DateofNormalEnd { set; get; }

        [Required(ErrorMessage = "Արժույթն ընտրված չէ։")]
        public string Currency { set; get; }

    }
}
