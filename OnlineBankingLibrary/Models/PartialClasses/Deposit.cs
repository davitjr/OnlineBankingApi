using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XBS
{
    public partial class Deposit
    {
        public double DigitalAvailabelBanlanaceAMD { get; set; }

        [JsonProperty]
        /// <summary>
        /// Պրոդուկտի նշում
        /// </summary>
        public ProductNote ProductNote { get; set; }
    }
}
