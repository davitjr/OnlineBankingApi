using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineBankingLibrary.Models.CustomerRegistration
{
    public class CustomerAuthenticationRequest
    {
        public DocumentType DocumentType { get; set; }
        public string DocumentValue { get; set; }
    }
}
