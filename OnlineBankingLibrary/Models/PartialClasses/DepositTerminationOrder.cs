using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XBS
{
    public partial class DepositTerminationOrder
    {
        /// <summary>
        /// Դադարեցվող Ավանդ
        /// </summary>
        [JsonProperty]
        public Deposit Deposit { get; set; }
    }
}
