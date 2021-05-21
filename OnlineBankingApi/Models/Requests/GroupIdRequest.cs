using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [GroupIdRequestValidation]
    public class GroupIdRequest
    {
        public int GroupId { get; set; }
    }
}
