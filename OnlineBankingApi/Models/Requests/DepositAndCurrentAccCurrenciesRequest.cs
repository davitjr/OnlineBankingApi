using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [DepositAndCurrentAccCurrenciesRequestValidation]
    public class DepositAndCurrentAccCurrenciesRequest
    {
        [Required(ErrorMessage = "Հայտի տեսակը պարտադիր է։")]
        public XBS.OrderType OrderType { set; get; }

        [Required(ErrorMessage = "Հայտի ենթատեսակը պարտադիր է։")]
        public byte OrderSubtype { set; get; }

        [Required(ErrorMessage = "Հաշվի տեսակը պարտադիր է։")]
        public OrderAccountType OrderAccountType { set; get; }
    }
}
