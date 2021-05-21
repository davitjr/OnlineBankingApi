using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using OnlineBankingApi.Filters;

namespace OnlineBankingApi.Models.Requests
{
    [ActiveDepositTypesRequestValidation]
    public class ActiveDepositTypesRequest
    {
        [Required(ErrorMessage = "Հաշվի տեսակն ընտրված չէ։")]
        public int AccountType { set; get; }

        [Required(ErrorMessage = "Հաճախորդի տեսակն ընտրված չէ։")]
        public int CustomerType { set; get; }
    }
}
