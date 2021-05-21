using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class AuthorizeByTokenRequest
    {
        public string OTP { get; set; }
        [Required]
        public string HostName { get; set; }

    }
}
