using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class POSStatementStatmentRequest
    {
        [Required(ErrorMessage = "Սկզբի ամսաթիվը նշված չէ։")]
        public DateTime DateFrom { set; get; }

        [Required(ErrorMessage = "Վերջի ամսաթիվը բացակայում է։")]
        public DateTime DateTo { set; get; }

        [Required(ErrorMessage = "Հաշվեհամար դաշտը բացակայում է։")]
        public string AccountNumber { set; get; }

        public string ExportFormat { set; get; } = "pdf";
    }
}
