using Newtonsoft.Json;

namespace XBS
{
    public partial class Account
    {
        /// <summary>
        /// Կցված ֆայլի առկայություն
        /// </summary>
        [JsonProperty]
        public bool HasContractFile { get; set; }

        /// <summary>
        /// ArCa մնացորդ
        /// </summary>
        [JsonProperty]
        public double? ArcaBalance { get; set; }

        /// <summary>
        /// Քարտի տեսակ
        /// </summary>
        [JsonProperty]
        public int CardSystem { get; set; }

        /// <summary>
        /// օվերդրաֆտ
        /// </summary>
        [JsonProperty]
        public CreditLine Overdraft { get; set; }

        public double DigitalAvailabelBanlanaceAMD { get; set; }

        [JsonProperty]
        /// <summary>
        /// Պրոդուկտի նշում
        /// </summary>
        public ProductNote ProductNote { get; set; }
    }
}
