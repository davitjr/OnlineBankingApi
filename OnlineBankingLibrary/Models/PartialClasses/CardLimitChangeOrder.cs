using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XBS
{
    public partial class CardLimitChangeOrder
    {
        [JsonProperty]
        public long ProductId { get; set; }
    }
}
