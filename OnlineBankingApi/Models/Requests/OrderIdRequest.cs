using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [OrderIdRequestValidation]
    public class OrderIdRequest
    {
        [Required(ErrorMessage = "Հանձնարարականի ունիկալ համարը պարտադիր է։")]
        public long OrderId { get; set; }
    }
}
