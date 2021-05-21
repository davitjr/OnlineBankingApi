using System.Collections.Generic;

namespace OnlineBankingLibrary.Models
{
    /// <summary>
    /// Հաճախորդի ակտիվ տոկենների ցուցադրման համար անհրաժեշտ տվյալներ
    /// </summary>
    public class CustomerTokenResponse
    {
        /// <summary>
        /// Is New Gemalto Hb User
        /// </summary>
        public bool IsNewHbUser { get; set; }

        /// <summary>
        /// Customer All Active Tokens
        /// </summary>
        public List<CustomerToken> Tokens { get; set; }
    }
    /// <summary>
    /// Հաճախորդի ակտիվ տոկենների ցուցադրման համար անհրաժեշտ տվյալներ
    /// </summary>
    public class CustomerToken
    {
        /// <summary>
        /// Token Number
        /// </summary>
        public string TokenSerial { get; set; }

        /// <summary>
        /// Device Description
        /// </summary>
        public string DeviceTypeDescription { get; set; }
    }
}
