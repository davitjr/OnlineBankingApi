using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models
{
    public class AuthorizationResponse
    {
        public string SessionId { get; set; }

        public int PasswordChangeRequirement { get; set; }

        public int UserPermission { get; set; }
        
        public string FullName { get; set; }

        public string FullNameEnglish { get; set; }

        public bool IsLastConfirmer { get; set; }

        public XBS.CustomerTypes CustomerType { get; set; }

        public int SecondConfirmRequirement { get; set; }

        public bool IsEmployee { get; set; }

        public string UserName { get; set; }

        public double? TotalAvailableBalance { get; set; }
    }
}
