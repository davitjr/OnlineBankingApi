using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineBankingLibrary.Models
{
    /// <summary>
    /// Հաճախորդի գրանցման համար անհրաժեշտ տվյալներ
    /// </summary>
    public class RegistrationCustomerData
    {
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Հաճախորդի էլեկտրոնային հասցե
        /// Այս դաշտով պահվում է "հաճախորդ" կարգավիճակով գրանցվողի մեր բազայի էլեկտրոնային հասցեն, 
        /// ինչպես նաև "հաճախորդ"-ից տարբվեր կարգավիճակով գրանցվողի իր կողմից նշված էլեկտոնային հասցեն
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Հաճախորդի հեռախոսահամար
        /// Այս դաշտով պահվում է "հաճախորդ" կարգավիճակով գրանցվողի մեր բազայի հեռախոսահամարը, 
        /// ինչպես նաև "հաճախորդ"-ից տարբվեր կարգավիճակով գրանցվողի իր կողմից նշված հեռախոսահամարը
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Login
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }


        /// <summary>
        ///Կրկնակի մուտքագրված  գաղտնաբառ  (Reentered Password)
        /// </summary>
        public string RePassword { get; set; }

        /// <summary>
        /// Գրանցման պրոցեսի ունիկալ տոկեն
        /// </summary>
        public string RegistrationToken { get; set; }

        /// <summary>
        /// Վերիֆիկացիայի կոդ
        /// </summary>
        public string VerificationCode { get; set; }

        /// <summary>
        /// Գրանցվողի հաճախորդի կարգավիճակ
        /// </summary>
        public int CustomerQuality { get; set; }

        public bool IsPhoneVerified { get; set; }

        public sbyte NumberFailedVerificationCount { get; set; }
    }
}

