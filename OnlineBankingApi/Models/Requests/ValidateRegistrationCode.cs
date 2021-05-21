using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    public class ValidateRegistrationCode
    {
        [Required(ErrorMessage = "Մուտքանունը բացակայում է։")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Գրանցման Բաժանորդի կոդը բացակայում է։")]
        public string RegistrationCode { get; set; }
    }
}
