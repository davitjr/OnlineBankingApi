using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBSInfo;

namespace OnlineBankingApi.Models.Requests
{
    public class GetOrderTypesRequest
    {
        public TypeOfHbProductTypes HbProductType { get; set; } = 0;
    }
}
