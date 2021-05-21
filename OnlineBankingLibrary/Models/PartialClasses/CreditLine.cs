using System;
using Newtonsoft.Json;


namespace XBS
{
    public partial class CreditLine
    {
        /// <summary>
        /// Կցված պայմանագրի առկայություն
        /// </summary>
        [JsonProperty]
        public bool HasContractFile { get; set; }
        /// <summary>
        /// Հաջորդ վճարման ամսաթիվ
        /// </summary>
        [JsonProperty]
        public DateTime? NextRepaymentDate { get; set; }
        /// <summary>
        /// Հաջորդ վճարման գումար
        /// </summary>
        [JsonProperty]
        public double NextRepaymentAmount { get; set; }

        /// <summary>
        /// Քարտի համակարգ
        /// </summary>
        [JsonProperty]
        public int CardSystem { get; set; }

        /// <summary>
        /// Ընդհանուր պարտք
        /// </summary>
        [JsonProperty]
        public double TotalDebt { get; set; }

        [JsonProperty]
        public double CardOverdraft { get; set; }

    }
}
