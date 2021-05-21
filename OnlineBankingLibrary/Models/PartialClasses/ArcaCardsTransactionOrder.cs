using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace XBS
{
    public partial class ArcaCardsTransactionOrder
    {
        /// <summary>
        /// Քարտի ունիկալ համար
        /// </summary>
        [JsonProperty]
        public long ProductId { get; set; }
    }
}
