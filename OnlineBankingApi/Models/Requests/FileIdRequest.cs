using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class FileIdRequest
    {
        [Required(ErrorMessage = "Ֆայլի ունիկալ համարը բացակայում է։")]
        public string FileId { get; set; }
    }
}
