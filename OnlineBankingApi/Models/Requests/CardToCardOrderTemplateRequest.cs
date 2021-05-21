using OnlineBankingApi.Filters;
using System.ComponentModel.DataAnnotations;

namespace OnlineBankingApi.Models.Requests
{
    [CardToCardOrderTemplateRequestValidation]
    public class CardToCardOrderTemplateRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public XBS.CardToCardOrderTemplate Template { set; get; }
    }
}
