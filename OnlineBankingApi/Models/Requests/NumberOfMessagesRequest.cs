using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [NumberOfMessagesRequestValidation]
    public class NumberOfMessagesRequest
    {
        public short MessageCount { set; get; }
        [Required(ErrorMessage = "Հաղորդագրությունների տեսակն ընտրված չէ։")]
        public MessageType Type { set; get; }
    }
}
