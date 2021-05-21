using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XBS;

namespace OnlineBankingApi.Models.Requests
{
    public class CommunalDetailsRequest
    {
        public short CommunalType { set; get; }
        public string AbonentNumber { set; get; }
        public short CheckType { set; get; }
        public string BranchCode { set; get; }
        public AbonentTypes AbonentType { set; get; } = AbonentTypes.physical;
    }
}
