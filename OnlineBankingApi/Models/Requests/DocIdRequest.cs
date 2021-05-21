using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [DocIdRequestValidation]
    public class DocIdRequest
    {
        [Required(ErrorMessage = "Գործարքի համարը լրացված չէ։")]
        [Range(1,long.MaxValue,ErrorMessage = "Գործարքի համարը պետք է 0-ից մեծ լինի։")]
        public long DocId { get; set; }
    }
}
