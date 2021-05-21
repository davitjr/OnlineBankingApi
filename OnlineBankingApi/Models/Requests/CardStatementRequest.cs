using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [CardStatementRequestValidation]
    public class CardStatementRequest
    {
       [Required(ErrorMessage = "Քարտի համարը պարտադիր է։")]
       public string CardNumber { set; get; }

        [Required(ErrorMessage = "Սկզբի ամսաթիվը պարտադիր է։")]
        public DateTime DateFrom { set; get; }

        [Required(ErrorMessage = "Նվազագույն գումար պետք է փոքր լինի առավելագույնից։։")]
        public DateTime DateTo { set; get; }

       public double MinAmount { set; get; }

       public double MaxAmount { set; get; }

       public string DebCred { set; get; }

        [Required(ErrorMessage = "Գործարքների քանակը պարտադիր է։")]
        public int TransactionsCount { set; get; }

       public short OrderByAscDesc { set; get; }
    }
}
