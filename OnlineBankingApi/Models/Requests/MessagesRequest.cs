using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [MessagesRequestValidation]
    public class MessagesRequest
    {
        public DateTime DateFrom { set; get; }
        public DateTime DateTo { set; get; }
        [Required(ErrorMessage = "Հաղորդագրությունների տեսակն ընտրված չէ։")]
        public MessageType Type { set; get; }
    }
}
