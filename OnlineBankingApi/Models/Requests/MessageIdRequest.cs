using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [MessageIdRequestValidation]
    public class MessageIdRequest
    {
        public int MessageId { get; set; }
    }
}
