using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class PrintCurrentAccStatementRequest
    {
        public string AccountNumber { set; get; }
        public string DateFrom { set; get; }
        public string DateTo { set; get; }
        public ushort IncludingExchangeRate { set; get; }
        public string ExportFormat { set; get; } = "pdf";
    }
}
