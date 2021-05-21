using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [ChangeTemplateStatusRequestValidation]
    public class ChangeTemplateStatusRequest
    {
        public int TemplateId { set; get; }
        [Required(ErrorMessage = "Ձևանմուշի կարգավիճակն ընտրված չէ։")]
        public TemplateStatus TemplateStatus { set; get; }
    }
}
