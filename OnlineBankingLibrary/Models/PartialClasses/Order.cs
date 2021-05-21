using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XBS
{
    public partial class Order
    {
        [JsonProperty]
        public string RejectReason { get; set; }
    }
}
