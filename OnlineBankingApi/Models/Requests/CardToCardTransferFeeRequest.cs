using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [CardToCardTransferFeeRequestValidation]
    public class CardToCardTransferFeeRequest
    {
        [Required(ErrorMessage = "Ելքագրվող քարտի համարը պարտադիր է։")]
        public string DebitCardNumber { set; get; }
        public string CreditCardNumber { set; get; }
        [Required(ErrorMessage = "Գումար դաշտը պարտադիր է։")]
        [Range(0,double.MaxValue)]
        public double Amount { set; get; }
        [Required(ErrorMessage = "Արժույթ դաշտը պարտադիր է։")]
        public string Currency { set; get; }
    }
}
