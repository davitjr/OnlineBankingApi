using Newtonsoft.Json;
using OnlineBankingLibrary.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace XBS
{

    public partial class Card
    {
        /// <summary>
        /// Քարտի կարգավիճակ
        /// </summary>
        [JsonProperty]
        public CardStatus CardStatus { get; set; }
        /// <summary>
        /// գաղտնաբառ
        /// </summary>
        [JsonProperty]
        public string Password { get; set; }
        /// <summary>
        /// SMS հեռախոսհամար
        /// </summary>
        [JsonProperty]
        public string CardSMSPhone { get; set; }
        /// <summary>
        /// Քարտին կցված էլ. փոստ
        /// </summary>
        [JsonProperty]
        public string CardEmail { get; set; }
        /// <summary>
        /// Քաղվածքի ստացման եղանակ
        /// </summary>
        [JsonProperty]
        public string ReportReceivingType { get; set; }
        /// <summary>
        /// կուտակված CASHBACK մինչև հարկումը`AMEX CASHBACK, AMEX BLUE քարտերի դեպքում
        /// </summary>
        [JsonProperty]
        public double CashBack { get; set; }
        /// <summary>
        /// կուտակված MR` AMEX GOLD, AMEX BLUE քարտերի դեպքում
        /// </summary>
        [JsonProperty]
        public double BonusBalance { get; set; }

        /// <summary>
        /// Հաճախորդի կողմից մուտքագրված CVV նշում
        /// </summary>
        [JsonProperty]
        public string CVVNote { get; set; }
        /// <summary>
        /// Քարտի սպասարկող մասնաճյուղի անվանում
        /// </summary>
        [JsonProperty]
        public string FilialName { get; set; }

        public double? DigitalAvailabelBanlanaceAMD { get; set; }

        [JsonProperty]
        /// <summary>
        /// Պրոդուկտի նշում
        /// </summary>
        public ProductNote ProductNote { get; set; }
    }
}

