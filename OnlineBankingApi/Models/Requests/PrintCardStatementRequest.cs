using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [PrintCardStatementRequestValidation]
    public class PrintCardStatementRequest
    {
        [Required(ErrorMessage = "Քարտի համարը լրացված չէ։")]
        public string CardNumber { set; get; }

        [Required(ErrorMessage = "Սկզբի ամսաթիվը լրացված չէ։")]
        public string DateFrom { set; get; }

        [Required(ErrorMessage = "Վերջի ամսաթիվը բացակայում է։")]
        public string DateTo { set; get; }

        public string ExportFormat { set; get; } = "pdf";
    }
}
