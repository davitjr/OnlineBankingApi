using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineBankingLibrary.Models
{
    /// <summary>
    /// Հաճախորդի տոկենի ստացման, հեռացման, փոխարինման համար անհրաժեշտ տվյալներ
    /// </summary>
    public class CustomerTokenInfo
    {
        /// <summary>
        /// Unique identificator
        /// </summary>
        public string SessionId  { get; set; }

        /// <summary>
        /// Source Type
        /// </summary>
        public XBS.SourceType SourceType { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        public byte Language { get; set; }

        /// <summary>
        /// One Time Password
        /// </summary>
        public string Otp { get; set; }

        /// <summary>
        /// Check if Otp verification is passed or not
        /// </summary>
        public bool Checked { get; set; }
        /// <summary>
        /// Customer Number
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// User Identificator
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// User Name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Customer Phone Number
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Customer Email
        /// </summary>
        public string Email { get; set; }
    }
}
