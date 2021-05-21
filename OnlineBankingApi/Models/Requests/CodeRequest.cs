using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [CodeRequestValidation]
    public class CodeRequest
    {
        [Required(ErrorMessage = "Բանկի կոդը պարտադիր է։")]
        public int Code { get; set; }
    }
}
