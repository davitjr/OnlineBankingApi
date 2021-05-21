using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineBankingLibrary
{
    /// <summary>
    /// Հաճախորդի կողմից գրանցման ժամանակ ներկայացվող բանկային պրոդուկտի տեսակներ
    /// </summary>
    public enum RegistrationProductType : short
    {
        /// <summary>
        /// Քարտի համար
        /// </summary>
        CardNumber = 1,

        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        AccountNumber = 2,
    }

    /// <summary>
    /// Ֆիզիկական անձ հաճախորդի գրանցման տեսակներ
    /// </summary>
    public enum TypeOfPhysicalCustomerRegistration : short
    {
        /// <summary>
        /// Գրանցում ID քարտով/անձնագրի համարով կամ ՀԾՀ-ով
        /// </summary>
        ByPersonalInformation = 1,

        /// <summary>
        /// Գրանցում քարտով/հաշվի համարով
        /// </summary>
        ByBankProductInformation = 2,
    }

    public enum RegistrationProcessSteps : short
    {
        SendSMSVerificationCodeByRegistrationToken = 1,

        CheckSMSVerificationCodeByRegistrationToken = 2,

        GenerateAcbaOnline = 3,
    }

    public enum RegistrationResult : short
    {
        /// <summary>
        /// Դիմողը ունի բանկում հաճախորդ կարգավիճակ և ամբողջական լրացված տվյալներ
        /// </summary>
        ClientWithCustomerQuality = 1,

        /// <summary>
        /// Հաճախորդը ունի գրանցված օնլայն հաշիվ
        /// </summary>
        ExistingOnlineUser = 2,

        /// <summary>
        /// Դիմողը ունի բանկում հաճախորդ կարգավիճակ, սակայն տվյալները ամբողջական չեն
        /// </summary>
        ClientHavingCustomerQualityWithInsufficientData = 3,

        /// <summary>
        /// Դիմողը ունի բանկում հաճախորդ կարգավիճակից տարբերվող այլ կարգավիճակ
        /// </summary>
        ClientWithNonCustomerQuality = 4,

        /// <summary>
        /// Դիմողը գտնված չէ Ekeng համակարգում
        /// </summary>
        NotFoundClient = 5,

        /// <summary>
        /// Հաճախորդի տվյալները ենթակա են թարմեցման
        /// </summary>
        UpdateExpired = 6,
    }

    /// <summary>
    /// Փաստաթղթերի տեսակներ
    /// </summary>
    public enum DocumentType : short
    {
        /// <summary>
        /// ՀՀ անձնագիր
        /// </summary>
        RApassport = 1,

        /// <summary>
        /// Նույնականացման քարտ
        /// </summary>
        IdentifierCard = 11,

        /// <summary>
        /// Հանրային ծառայության համարանիշ
        /// </summary>
        SocialServiceNumber = 56,

        /// <summary>
        /// ՀՀ բիոմետրիկ անձնագիր
        /// </summary>
        BiometricPassport = 88
    }

    public enum CustomerAuthenticationResult : short
    {
        /// <summary>
        /// Բանկում կարգավիճակ չունեցող հաճախորդ
        /// </summary>
        NonCustomer = 0,

        /// <summary>
        /// Բանկում կարգավիճակ ունեցող հաճախորդը չունի կցված փաստթղթեր
        /// </summary>
        CustomerWithNoAttachments = 1,

        /// <summary>
        /// Բանկում կարգավիճակ ունեցող հաճախորդ
        /// </summary>
        CustomerWithAttachment = 2,

        /// <summary>
        /// Հաճախորդը, որը ունի օնլայն բանկինգ
        /// </summary>
        CustomerWithOnlineBanking = 3
    }

    public enum CustomerAuthenticationInfoType : short
    {
        /// <summary>
        /// Նկար
        /// </summary>
        Photo = 1,

        /// <summary>
        /// Փաստաթուղթ
        /// </summary>
        Document = 2,

        /// <summary>
        /// Դատարկ
        /// </summary>
        Empty = 0
    }

    public enum TypeOfAttachments : short
    {
        jpg = 1,
        pdf = 2
    }

    public enum CustomerRegistrationVerificationSMSTypes : short
    {
        OnlyNumbers = 1,
        OnlyLetters = 2,
        NumbersAndLetters = 3
    }

    public enum ApprovalOrderType : int
    {
        /// <summary>
        /// Չնշված
        /// </summary>
        NotDefined = 0,
        /// <summary>
        /// Հաստատում/ուղարկում է տոկենի ապաբլոկավորման հայտը
        /// </summary>
        HBServletRequestOrder = 1,
        /// <summary>
        /// Փոխանցում ՀՀ տարածքում
        /// </summary>
        PaymentOrder = 2,
        /// <summary>
        /// Նոր քարտի հայտ
        /// </summary>
        PlasticCardOrder = 3,
        /// <summary>
        /// Կոմունալ վճարում
        /// </summary>
        UtilityPaymentOrder = 4,
        /// <summary>
        /// Վարկի մարում
        /// </summary>
        MatureOrder = 5,
        /// <summary>
        /// Տեղեկանքի ստացում
        /// </summary>
        ReferenceOrder = 6,
        /// <summary>
        /// Ավանդի գրավով վարկի ձևակերպում
        /// </summary>
        LoanProductOrder = 7,
        /// <summary>
        /// Արագ համակարգերով ստացված փոխանցումներ
        /// </summary>
        ReceivedFastTransferPaymentOrder = 8,
        /// <summary>
        /// Հաշվի փակում
        /// </summary>
        AccountClosingOrder = 9,
        /// <summary>
        /// SWIFT հաղորդագրության պատճենի ստացում
        /// </summary>
        SwiftCopyOrder = 10,
        /// <summary>
        /// Լիազորագրի մուտքագրման հայտ
        /// </summary>
        CredentialOrder = 11,
        /// <summary>
        /// Ավանդի ձևակերպում
        /// </summary>
        DepositOrder = 12,
        /// <summary>
        /// Ընթացիկ հաշվի բացում
        /// </summary>
        AccountOrder = 13,
        /// <summary>
        /// Գումարի ստացում
        /// </summary>
        CashOrder = 15,
        /// <summary>
        /// Վարկային գծի դադարեցում
        /// </summary>
        CreditLineTerminationOrder = 16,
        /// <summary>
        /// Քարտի փակում
        /// </summary>
        CardClosingOrder = 17,
        /// <summary>
        /// Տվյալների խմբագրում
        /// </summary>
        CustomerDataOrder = 18,
        /// <summary>
        /// Քաղվածքի էլեկտրոնային ստացում
        /// </summary>
        StatmentByEmailOrder = 19,
        /// <summary>
        /// Ավանդի դադարեցում
        /// </summary>
        DepositTerminationOrder = 20,
        /// <summary>
        /// Կանխիկ մուտք ռեեստրով
        /// </summary>
        ReestrTransferOrder = 21,
        /// <summary>
        /// Քարտի բլոկավորման/ապաբլոկավորման հայտ
        /// </summary>
        ArcaCardsTransactionOrder = 22,
        /// <summary>
        /// Քարտից քարտ փոխանցման հայտ
        /// </summary>
        CardToCardOrder = 23,
        /// <summary>
        /// Քարտի սահմանաչափերի փոփոխման հայտ
        /// </summary>
        CardLimitChangeOrder = 24,
        /// <summary>
        /// Պարբերական փոխանցում
        /// </summary>
        PeriodicPaymentOrder = 25,
        /// <summary>
        /// Միջազգային փոխացում
        /// </summary>
        InternationalPaymentOrder = 26,
        /// <summary>
        /// Ընթացիկ հաշվի վերաբացում
        /// </summary>
        AccountReOpenOrder = 27,
        /// <summary>
        /// Քարտի SMS ծառայություն 
        /// </summary>
        PlasticCardSmsServiceOrder = 28,
        /// <summary>
        /// Գործարքից հրաժարում
        /// </summary>
        RemovalOrder = 29,
        /// <summary>
        /// Միաժամանակ 1-ից ավելի գործարքների հաստատում
        /// </summary>
        MultipleOrders = 30,
        /// <summary>
        /// Պարբերական փոխանցման դադարեցման հայտի հաստատում
        /// </summary>
        PeriodicTerminationOrder = 31,
        /// <summary>
        /// Պարբերական փոխանցման խմբագրման հայտ
        /// </summary>
        PeriodicDataChangeOrder = 32,
        /// <summary>
        /// Քարտի ակտիվացում
        /// </summary>
        CardActivationOrder = 33
    }
}
