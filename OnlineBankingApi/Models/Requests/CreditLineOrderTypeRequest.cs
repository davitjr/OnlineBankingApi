using OnlineBankingApi.Filters;
using System.ComponentModel.DataAnnotations;

namespace OnlineBankingApi.Models.Requests
{
    [CreditLineOrderTypeRequestValidation]
    public class CreditLineOrderTypeRequest
    {
        [Required(ErrorMessage = "Հայտի տեսակի նշված չէ։")]
        public XBS.OrderType OrderType { get; set; }
    }
}
