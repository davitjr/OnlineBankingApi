using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [SaveReestrTransferOrderRequestValidation]
    public class SaveReestrTransferOrderRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public ReestrTransferOrder Order { set; get; }
        public string FileId { set; get; }
    }
}
