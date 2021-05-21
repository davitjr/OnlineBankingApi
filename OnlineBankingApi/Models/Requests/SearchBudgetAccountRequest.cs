using OnlineBankingApi.Filters;
using System.ComponentModel.DataAnnotations;
using XBS;

namespace OnlineBankingApi.Models.Requests
{
    [SearchBudgetAccountRequestValidation]
    public class SearchBudgetAccountRequest
    {
        [Required(ErrorMessage = "Հարցման պարունակությունը դատարկ է։")]
        public SearchBudgetAccount SearchAccount { set; get; }
    }
}

