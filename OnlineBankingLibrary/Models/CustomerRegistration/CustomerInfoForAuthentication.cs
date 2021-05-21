using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineBankingLibrary.Models
{
    public class CustomerInfoForAuthentication
    {
        public CustomerInfoForAuthentication()
        {
            ProcessResultCode = new CustomerAuthenticationResult();
            Data = new List<KeyValuePair<string, string>>();
        }

        public ulong CustomerNumber { get; set; }

        public CustomerAuthenticationInfoType TypeOfDocument { get; set; }

        public List<KeyValuePair<string, string>> Data { get; set; }

        public CustomerAuthenticationResult ProcessResultCode { get; set; }

        public string ResultDescription { get; set; }

    }
}
