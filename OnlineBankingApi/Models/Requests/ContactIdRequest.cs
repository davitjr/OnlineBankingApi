using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class ContactIdRequest
    {
        [Range(1,ulong.MaxValue,ErrorMessage = "Կոնտակտի ունիկալ համարը սխալ է։")]
        public ulong ContactId { get; set; }
    }
}
