using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CheckExcelRowsRequestValidation]
    public class CheckExcelRowsRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public List<ReestrTransferAdditionalDetails> ReestrTransferAdditionalDetails { set; get; }
        public string DebetAccount { set; get; }

        public long OrderId { get; set; }
    }
}
