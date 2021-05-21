using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XBS
{
    public partial class PlasticCardOrder
    {
        [JsonProperty]
        public string ServiceFeePeriodicityTypeDescription { get; set; }
    }
}
