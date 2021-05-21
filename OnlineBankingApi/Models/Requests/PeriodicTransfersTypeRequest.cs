using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class PeriodicTransfersTypeRequest
    {
        public XBS.PeriodicTransferTypes PeriodicType { get; set; }
    }
}
