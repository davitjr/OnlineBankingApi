using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [MessageTypeRequestValidation]
    public class MessageTypeRequest
    {
        [Required(ErrorMessage = "Հաղորդագրության տեսակը նշված չէ։")]
        public XBS.MessageType Type { set; get; }
    }
}
