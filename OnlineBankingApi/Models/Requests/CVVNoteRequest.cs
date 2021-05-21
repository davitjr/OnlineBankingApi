using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [CVVNoteRequestValidation]
    /// <summary>
    /// Հաճախորդի կողմից մուտքագրված CVV
    /// </summary>
    public class CVVNoteRequest
    {
        [Required(ErrorMessage = "Պրոդուկտի ունիկալ համարը բացակայում է։")]
        /// <summary>
        /// Քարտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Հաճախորդի կողմից մուտքագրված CVV կոդ (որպես նշում)
        /// </summary>
        public string CVVNote { get; set; }
    }
}
