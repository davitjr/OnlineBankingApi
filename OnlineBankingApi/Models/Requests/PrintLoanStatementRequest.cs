using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class PrintLoanStatementRequest
    {
        public ulong ProductId { set; get; }
        public DateTime DateFrom { set; get; }
        public DateTime DateTo { set; get; }
        public int Language { get; set; }
        public string ExportFormat { set; get; } = "pdf";
    }
}
