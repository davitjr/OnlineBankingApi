using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using OnlineBankingApi.Filters;

namespace OnlineBankingApi.Models.Requests
{
    [AccountForOrderRequestValidation]
    public class AccountForOrderRequest
    {
        [Required(ErrorMessage = "Հայտի տեսակն ընտրված չէ։")]
        public short OrderType { set; get; }

        [Required(ErrorMessage = "Հայտի ենթատեսակն ընտրված չէ։")]
        public byte OrderSubType { set; get; }

        [Required(ErrorMessage = "Հաշվի տեսակն ընտրված չէ։")]
        public byte AccountType { set; get; }
    }
}
