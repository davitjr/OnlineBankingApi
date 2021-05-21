using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class GUIDRequest
    {
        [Required(ErrorMessage = "Ունիկալ համարը մուտքագրված չէ։")]
        public string Id { get; set; }
    }
}
