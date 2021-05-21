using OnlineBankingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class CustomerInfoforAuthenticationRequest
    {
        public DocumentType DocumentType { set; get; }
        public string DocumentValue { set; get; }
    }
}
