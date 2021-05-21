using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineBankingLibrary.Models
{
    public class CustomerRegistrationProcessResult
    {
        public CustomerRegistrationProcessResult()
        {
            RegistrationResponseData = new Dictionary<string, string>();
        }

        public Dictionary<string, string> RegistrationResponseData { get; set; }

        public RegistrationResult RegistrationResult { get; set; }

        public string ResultDescription { get; set; }
    }
}
