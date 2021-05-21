using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [TemplateIdRequestValidation]
    public class TemplateIdRequest
    {
        [Required(ErrorMessage = "Ձևանմուշի ունիկալ համարը պարտադիր է։")]
        public int TemplateId { get; set; }
    }
}
