using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBS;

namespace OnlineBankingApi.Models.Requests
{
    [ProductQualityFilterRequestValidation]
    public class ProductQualityFilterRequest
    {       
        [Required(ErrorMessage = "Պրոդուկտի կարգավիճակի ֆիլտրն ընտրված չէ։")]
        public ProductQualityFilter Filter { set ; get; }
    }
}
