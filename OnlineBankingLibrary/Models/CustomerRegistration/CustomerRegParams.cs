using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineBankingLibrary.Models
{
    /// <summary>
    /// Ֆիզիկական անձ հաճախորդների գրանցման համար անհրաժեշտ տվյալներ
    /// </summary>
    public class CustomerRegParams
    {
        /// <summary>
        /// Հաճախորդի կողմից ընտրված գրանցման տեսակ 
        /// Օր․՝ գրանցում անձնական ինֆորմացիայի կամ բանկի կողմից արդեն տրամադրված պրոդուկտի տվյալների միջոցով
        /// </summary>
        public TypeOfPhysicalCustomerRegistration RegType { get; set; }

        /// <summary>
        /// Հաճախորդի կողմից ընտրված տվյալների լրացման տեսակ
        /// Օր․՝ տվյալների մուտքագրում ձեռքով կամ տեսախցիկի միջոցով
        /// </summary>
        //public TechnicalTypeOfPhysicalCustomerRegistration TechRegType { get; set; }

        /// <summary>
        /// Հաճախորդի կողմից անձնական ինֆորմացիայի միջոցով գրանցվելու ժամանակ ընտրված փաստաթղթի տեսակ
        /// Օր․՝ անձնագիր կամ սոցիալական քարտ
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Հաճախորդի կողմից անձնական ինֆորմացիայի միջոցով գրանցվելու ժամանակ ընտրված փաստաթղթի իդենտիֆիկացնող տվյալ
        /// Օր․՝ անձնագրի համար կամ հանրային ծառայության համարանիշ
        /// </summary>
        public string DocumentValue { get; set; }

        /// <summary>
        /// Հաճախորդի կողմից բանկային պրոդուկտի միջոցով գրանցման ժամանակ ընտրված բանկային պրոդուկտի տեսակ
        /// Օր․՝ քարտ կամ հաշիվ
        /// </summary>
        public RegistrationProductType ProductType { get; set; }

        /// <summary>
        /// Հաճախորդի կողմից բանկային պրոդուկտի միջոցով գրանցման ժամանակ ընտրված բանկային պրոդուկտի տվյալ
        /// Օր․՝ քարտի համար կամ հաշվեհամար
        /// </summary>
        public string ProductValue { get; set; }
    }
}
