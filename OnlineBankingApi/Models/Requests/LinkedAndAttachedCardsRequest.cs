using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [LinkedAndAttachedCardsRequestValidation]
    public class LinkedAndAttachedCardsRequest
    {
        public ulong ProductId { set; get; }
        public ProductQualityFilter ProductFilter { set; get; } = ProductQualityFilter.Opened;
    }
}
