using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [GenericIdRequestValidation]
    public class GenericIdRequest
    {
        [Required(ErrorMessage = "Id ունիկալ համարը լրացված չէ։")]
        public int Id { get; set; }

    }
}
