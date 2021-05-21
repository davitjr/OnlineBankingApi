using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class AuthorizeByPasswordRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        [Required]
        public string HostName { get; set; }
        public bool ForUnlocking { get; set; }
    }
}
