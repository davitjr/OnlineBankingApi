using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineBankingLibrary.Models.CustomerRegistration
{
    public class RegistrationActionResult
    {
        public ResultCode ResultCode { get; set; }
        public string Description { get; set; }
    }

    public enum ResultCode : short
    {
        Failed = 0,
        Normal = 1,
        ValidationError = 2
    }
}
