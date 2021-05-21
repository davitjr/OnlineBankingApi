using OnlineBankingApi.Filters;
using System.ComponentModel.DataAnnotations;
using XBS;

namespace OnlineBankingApi.Models.Requests
{
    [OrderTypeValidation]
    public class OrderTypeRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public OrderType OrderType { get; set; }

    }
}


