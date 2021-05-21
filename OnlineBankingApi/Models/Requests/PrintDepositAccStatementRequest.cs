using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class PrintDepositAccStatementRequest
    {
        [Required(ErrorMessage = "Պրոդուկտի ունիկալ համարը պարտադիր է։")]
        public long ProductId { set; get; }

        [Required(ErrorMessage = "Հաշվեհամարը պարտադիր է։")]
        public string AccountNumber { set; get; }

        [Required(ErrorMessage = "Սկզբի ամսաթիվը պարտադիր է։")]
        public string DateFrom { set; get; }

        [Required(ErrorMessage = "Նվազագույն գումար պետք է փոքր լինի առավելագույնից։։")]
        public string DateTo { set; get; }

        public ushort IncludingExchangeRate { set; get; }
        public string ExportFormat { set; get; } = "pdf";
    }
}
