using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [SearchCommunalRequestValidation]
    public class SearchCommunalRequest
    {
        public XBS.SearchCommunal SearchCommunal { set; get; }
    }
}
