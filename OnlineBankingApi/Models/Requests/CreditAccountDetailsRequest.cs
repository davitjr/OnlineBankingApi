using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class CreditAccountDetailsRequest
    {
        [Required(ErrorMessage = "Քարտի համարը լրացված չէ։")]
        public long ProductId { set; get; }

        public string ExportFormat { set; get; } = "pdf";
    }
}
