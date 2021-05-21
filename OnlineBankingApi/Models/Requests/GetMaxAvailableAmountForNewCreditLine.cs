using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [AvailableAmountRequestValidation]
    public class GetMaxAvailableAmountForNewCreditLine
    {
        [Required(ErrorMessage = "Քարտի ունիկալ համարն ընտրված չէ")]
        public ulong ProductId { get; set; }

        [Required(ErrorMessage = "Վարկային գծի տեսակը ընտրված չէ")]
        public int CreditLineType { get; set; }

        [Required(ErrorMessage = "Նշված չէ առկա են պարտադիր մուտքեր, թե ոչ")]
        public bool ExistRequiredEntries { get; set; }

        [Required(ErrorMessage = "Նշված չէ գրավադրվող արժույթը")]
        public string ProvisionCurrency { get; set; }
    }
}
