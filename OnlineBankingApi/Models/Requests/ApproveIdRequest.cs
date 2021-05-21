using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class ApproveIdRequest
    {
        public long Id { get; set; }
        public string OTP { get; set; }
    }
}
