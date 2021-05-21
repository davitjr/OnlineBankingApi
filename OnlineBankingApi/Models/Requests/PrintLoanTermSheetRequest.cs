using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [PrintLoanTermSheetRequestValidation]
    public class PrintLoanTermSheetRequest
    {
        [Range(1,3,ErrorMessage = "Վարկի տեսակն ընտրված չէ։")]
        public byte LoanType { set; get; }

        public long Orderid { get; set; }
    }
}
