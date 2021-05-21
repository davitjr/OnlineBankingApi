using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [IdRequestValidation]
    public class IdRequest
    {
        [Required(ErrorMessage = "Հանձնարարականի ունիկալ համարը պարտադիր է։")]
        public long Id { get; set; }
    }
}
