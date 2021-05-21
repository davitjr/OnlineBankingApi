using System;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
   [RedemptionAmountRequestValidation]
    public class RedemptionAmountRequest
    {
        public double StartCapital { set; get; }
        public double InterestRate { set; get; }
        public DateTime DateOfBeginning { set; get; }
        public DateTime DateOfNormalEnd { set; get; }
        public DateTime FirstRepaymentDate { set; get; }
    }
}
