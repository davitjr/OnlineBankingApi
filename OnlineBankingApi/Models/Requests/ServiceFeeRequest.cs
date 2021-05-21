using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XBS;

namespace OnlineBankingApi.Models.Requests
{
    public class ServiceFeeRequest
    {
        public OrderType Type{ get; set; }
        public int Urgent { get; set; } = default;
    }
}
