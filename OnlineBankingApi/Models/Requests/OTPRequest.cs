using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class OTPRequest
    {
        [Required(ErrorMessage ="Մեկանգամյա թվային Բաժանորդի կոդը բացակայում է։")]
        public string OTP { get; set; }
    }
}
