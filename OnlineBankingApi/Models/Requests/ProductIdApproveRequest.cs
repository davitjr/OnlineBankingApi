
using OnlineBankingApi.Filters;
using System.ComponentModel.DataAnnotations;


namespace OnlineBankingApi.Models.Requests
{
    [ProductIdApproveRequestValidation]
    public class ProductIdApproveRequest
    {
        [Required(ErrorMessage = "Պրոդուկտի ունիկալ համարը լրացված չէ։")]
        public ulong ProductId { get; set; }

        /// <summary>
        /// Թվային կոդ
        /// </summary>
        public string OTP { get; set; }
    }
}


