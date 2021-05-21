using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CredentialTypesRequestValidation]
    public class CredentialTypesRequest
    {
        public int TypeOfCustomer { set; get; }
        public int CustomerFilialCode { set; get; }
        public int UserFilialCode { set; get; }
    }
}
