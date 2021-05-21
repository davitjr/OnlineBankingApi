using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XBS
{
    public partial class Loan
    {
        [JsonProperty]
        public bool HasContractFile { get; set; }

        [JsonProperty]
        public double RepaymentAmount { get; set; }
    }
}
