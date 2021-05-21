using OnlineBankingApi.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [PasswordResetRequestValidation]
    public class PasswordResetRequest
    {
        /// <summary>
        /// Օգտագործողի մուտքանուն
        /// </summary>
        [Required(ErrorMessage = "Մուտքանունը լրացված չէ։")]
        public string UserName { get; set; }

        /// <summary>
        /// Թվային կոդ
        /// </summary>
        [Required(ErrorMessage = "Մեկանգամյա թվային կոդը մուտքագրված չէ։")]
        public string OTP { get; set; }
    }
}
