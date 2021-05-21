using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [ProductIdRequestValidation]
    public class ProductIdRequest
    {
        [Required(ErrorMessage = "Պրոդուկտի ունիկալ համարը լրացված չէ։")]
        public ulong ProductId { get; set; }
    }
}
