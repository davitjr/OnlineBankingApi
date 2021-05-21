using OnlineBankingApi.Filters;
using OnlineBankingLibrary.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    [CardTariffsByCardTypeRequestValidation]
    public class CardTariffsByCardTypeRequest
    {
        /// <summary>
        /// Քարտի տեսակ
        /// </summary>
        [Required(ErrorMessage ="Քարտի տեսակը պարտադիր է։")]
        [Range(1,100,ErrorMessage = "Քարտի տեսակը սխալ է։")]
        public ushort CardType { set; get; }
       
        /// <summary>
        /// Հաճախականություն (անհրաժեշտ է փոխանցել AMEX քարտատեսակների դեպքում)
        /// </summary>
        public short PeriodicityType { set; get; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        [Required(ErrorMessage = "Արժույթ դաշտը պարտադիր է։")]
        public string Currency { get; set; }
    }
}
