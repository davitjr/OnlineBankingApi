using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [OrderGroupRequestValidation]
    public class OrderGroupRequest
    {
        [Required(ErrorMessage = "Խմբի կարգավիճակը նշված չէ։")]
        public XBS.OrderGroupStatus Status { set; get; } = XBS.OrderGroupStatus.Active;

        [Required(ErrorMessage = "Խմբի տեսակը նշված չէ։")]
        public XBS.OrderGroupType GroupType { set; get; } = XBS.OrderGroupType.CreatedByCustomer;
    }
}
