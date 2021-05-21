using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [BusinesDepositOptionRateRequestValidation]
    public class BusinesDepositOptionRateRequest
    {
        [Range(1,6,ErrorMessage = "Ավանդի օպցիան ընտրված չէ։")]
        public ushort DepositOption { set; get; }

        [Required(ErrorMessage ="Արժույթն ընտրված չէ։")]
        public string Currency { set; get; }
    }
}
