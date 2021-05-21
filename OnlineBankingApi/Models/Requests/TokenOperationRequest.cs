using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class TokenOperationRequest
    {
        /// <summary>
        /// User Token Number
        /// </summary>
        public string TokenSerial { get; set; }
        /// <summary>
        /// Custom generated Otp from sms to verify user
        /// </summary>
        public string Otp { get; set; }
        /// <summary>
        /// If true we migrate users to gemalto if false we replace old token for new (SaveAndApproveTokenReplacementOrder)
        /// </summary>
        public bool IsNewHbUser { get; set; }
    }
    public class TokenOperationRequestWithAuthorization : TokenOperationRequest
    {
        /// <summary>
        /// User Name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }
    }
}
