using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XBS
{
    public partial class LoanProductOrder
    {
        [JsonProperty]
        public string ProductCardNumber { get; set; }
        [JsonProperty]
        public string MandatoryPaymentDescription { get; set; }
        [JsonProperty]
        public int ProductCardSystem { get; set; }
    }
}
