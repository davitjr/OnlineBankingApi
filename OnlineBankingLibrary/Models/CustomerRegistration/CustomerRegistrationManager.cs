using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ACBAServiceReference;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.CompilerServices;
using OnlineBankingLibrary.Models.CustomerRegistration;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using CustomerEmail = ACBAServiceReference.CustomerEmail;
using CustomerPhone = ACBAServiceReference.CustomerPhone;
using Email = ACBAServiceReference.Email;
using KeyValue = ACBAServiceReference.KeyValue;
using Person = ACBAServiceReference.Person;
using Phone = ACBAServiceReference.Phone;
using PhysicalCustomer = ACBAServiceReference.PhysicalCustomer;
using Result = ACBAServiceReference.Result;
using User = ACBAServiceReference.User;

namespace OnlineBankingLibrary.Models
{
    public class CustomerRegistrationManager
    {
        private readonly ACBAOperationService _acbaOperationService;
        private readonly XBService _xbService;
        private readonly XBManagementService _xbManagementService;
        private readonly SMSMessagingService _smsMessagingService;
        private readonly XBInfoService _xbInfoService;
        private readonly CacheManager _cache;
        private readonly CacheHelper _cacheHelper;
        private readonly IConfiguration _config;
        private readonly OnlineBankingRegistrationManager _onlineBankingRegistrationManager;

        #region Properties
        /// <summary>
        /// Օգտագործվում է գրացման ամենասկզբում, երբ հարցում է գալիս, որը պարունակում է, թե ինչպիսի եղանակով է ցանկանում հաճախորդը գրանցվել
        /// Ֆիզիկական անձ հաճախորդի գրանցման անհրաժեշտ տվյալներ
        /// </summary>
        public CustomerRegParams RegParams { get; set; }

        /// <summary>
        /// Հաճախորդի գրանցման համար անհրաժեշտ տվյալներ
        /// </summary>
        private RegistrationCustomerData CustomerData { get; set; } = new RegistrationCustomerData();

        /// <summary>
        /// Ինիցիալացնում է ֆիզիկական անձի գրանցման համար անհրաժեշտ տվյալները։
        /// </summary>
        /// <param name="regParams"></param>
        public CustomerRegistrationManager(ACBAOperationService acbaOperationService, XBService xbService, XBManagementService xbManagementService, SMSMessagingService smsMessagingService, XBInfoService xbInfoService, OnlineBankingRegistrationManager onlineBankingRegistrationManager, CacheManager cache, CacheHelper cacheHelper, IConfiguration config)
        {
            _acbaOperationService = acbaOperationService;
            _xbService = xbService;
            _xbManagementService = xbManagementService;
            _smsMessagingService = smsMessagingService;
            _xbInfoService = xbInfoService;
            _onlineBankingRegistrationManager = onlineBankingRegistrationManager;
            _cache = cache;
            _cacheHelper = cacheHelper;
            _config = config;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Հաճախորդի գրանցում անձնական տվյալներով
        /// Նույնականացնում է ֆիզիկական անձին անձնական տվյալների հիման վրա։
        /// Եթե Նորք/Էկենգից վերադարձված հաճախորդը առկա չէ բանկի բազայում, ապա գրանցում է։ 
        /// </summary>
        /// <returns></returns>
        public CustomerRegistrationProcessResult RegistrateByPersonalInformation()
        {
            var result = new CustomerRegistrationProcessResult();
            PhysicalCustomer physicalCustomer = null;
            CustomerIdentificationResult identificationResult = null;

            if (!String.IsNullOrEmpty(this.RegParams.DocumentValue))
            {
                var notIdentifiedCustomer = new PhysicalCustomer() { person = new Person() { documentList = new List<CustomerDocument>() } };
                notIdentifiedCustomer.person.documentList.Add(new CustomerDocument() { documentGroup = new KeyValue() { key = 1 }, documentType = new KeyValue() { key = (short)this.RegParams.DocumentType }, documentNumber = this.RegParams.DocumentValue });

                try
                {
                    identificationResult = _acbaOperationService.IdentifyCustomer(notIdentifiedCustomer);
                }
                catch
                {
                    result.RegistrationResult = RegistrationResult.NotFoundClient;
                    result.ResultDescription = "Գրանցումը հնարավոր չէ կատարել:  Փորձեք մի փոքր ուշ կամ կապ հաստատեք Բանկի հետ:";
                    return result;
                }
                physicalCustomer = _acbaOperationService.GetCustomer(identificationResult.CustomerNumber);
            }

            result = RegistratePhysicalCustomer(physicalCustomer);

            return result;
        }

        /// <summary>
        /// Հաճախորդի գրանցում քարտով կամ ընթացիկ հաշվով
        /// </summary>
        /// <returns></returns>
        public CustomerRegistrationProcessResult RegistrateByBankProductInformation()
        {
            var result = new CustomerRegistrationProcessResult();
            var customer = new PhysicalCustomer();

            if (!String.IsNullOrEmpty(this.RegParams.ProductValue))
            {
                if (this.RegParams.ProductType == RegistrationProductType.AccountNumber)
                {
                    try
                    {
                        customer.customerNumber = _xbService.GetAccountCustomerNumber(this.RegParams.ProductValue);
                    }
                    catch
                    {
                        //Քանի որ չգտնելու դեպքում xbs-ից throw է արվում exception
                        customer.customerNumber = 0;
                    }
                }
                else if (this.RegParams.ProductType == RegistrationProductType.CardNumber)
                    customer.customerNumber = _xbService.GetCardCustomerNumber(this.RegParams.ProductValue);


                if (customer.customerNumber == default(ulong))
                    customer = null;
                else
                {
                    customer = _acbaOperationService.GetCustomer(customer.customerNumber);
                }
            }
            else
                customer = null;

            if (customer == null)
            {
                result.RegistrationResult = RegistrationResult.NotFoundClient;
                result.ResultDescription = "Մուտքագրվել են սխալ տվյալներ: Խնդրում ենք կրկին մուտքագրել:";
                return result;
            }
            result = RegistratePhysicalCustomer(customer);
            return result;
        }

        public RegistrationActionResult ContinueRegistrationProcess(RegistrationCustomerData externalRegData, RegistrationProcessSteps step)
        {
            var result = new RegistrationActionResult();
            result.ResultCode = CustomerRegistration.ResultCode.Failed;
            this.CustomerData = _cache.Get<string, RegistrationCustomerData>("RegistrationCustomerData_" + externalRegData.RegistrationToken);
            if (this.CustomerData != null)
            {
                _cache.UpdateExpiration("RegistrationCustomerData_" + externalRegData.RegistrationToken, 15);
                switch (step)
                {
                    case RegistrationProcessSteps.SendSMSVerificationCodeByRegistrationToken:
                        if (this.CustomerData.CustomerQuality != 1 && externalRegData.PhoneNumber.Length == 8 && externalRegData.PhoneNumber.All(c => Char.IsDigit(c)))
                            this.CustomerData.PhoneNumber = "374" + externalRegData.PhoneNumber;

                        this.CustomerData.VerificationCode = this.SendVerificationCode(this.CustomerData.PhoneNumber, 5, CustomerRegistrationVerificationSMSTypes.NumbersAndLetters);

                        result.ResultCode = ResultCode.Normal;
                        break;
                    case RegistrationProcessSteps.CheckSMSVerificationCodeByRegistrationToken:
                        this.CustomerData.IsPhoneVerified = this.CustomerData.VerificationCode == externalRegData.VerificationCode;

                        if(this.CustomerData.NumberFailedVerificationCount < 3)
                        {
                            if (this.CustomerData.IsPhoneVerified)
                            {
                                this.CustomerData.VerificationCode = String.Empty;
                                if (CustomerData.CustomerQuality != 1)
                                    result = this.AddPhoneForCustomer(this.CustomerData.PhoneNumber, this.CustomerData.CustomerNumber);
                                else
                                    result.ResultCode = ResultCode.Normal;
                            }
                            else
                            {
                                result.Description = "Մուտքագրվել է սխալ մեկանգամյա գաղտնաբառ: Մուտքագրեք Բանկում գրանցված Ձեր հեռախոսահամարին ուղարկված մեկանգամյա գաղտնաբառը:";
                                this.CustomerData.NumberFailedVerificationCount++;
                            }
                        }
                        else
                        {
                            _cache.Remove("RegistrationCustomerData_" + externalRegData.RegistrationToken);
                            result.ResultCode = ResultCode.ValidationError;
                            result.Description = "Դուք գերազանցել եք տվյալների սխալ մուտքագրման սահմանաչափը: Գործընթացը ավարտված է:";
                        }
                        
                        break;
                    case RegistrationProcessSteps.GenerateAcbaOnline:
                        this.CustomerData.UserName = externalRegData.UserName;
                        this.CustomerData.Password = externalRegData.Password;

                        if (this.CustomerData.CustomerQuality != 1)
                            this.CustomerData.Email = externalRegData.Email;

                        RegistrationActionResult validationResult = _onlineBankingRegistrationManager.ValidateAcbaOnlineCreating(this.CustomerData);
                        if (validationResult.ResultCode == ResultCode.Normal)
                        {
                            if (this.CustomerData.CustomerQuality != 1)
                            {
                                RegistrationActionResult  saveEmailResult = this.AddEmailForCustomer(this.CustomerData.Email, this.CustomerData.CustomerNumber);

                                if (saveEmailResult.ResultCode == ResultCode.Normal)
                                    result = _onlineBankingRegistrationManager.GenerateAcbaOnline(this.CustomerData);
                            }
                            else
                            {
                                result = _onlineBankingRegistrationManager.GenerateAcbaOnline(this.CustomerData);
                            }
                        }
                        else
                            result = validationResult;

                        break;
                }
            }
            else
            {
                result.Description = "Initilized customerdata is null";
            }
            return result;
        }

        private string SendVerificationCode(string phoneNumber, byte verificationCodeLength, CustomerRegistrationVerificationSMSTypes type)
        {
            var rnd = new Random();
            string verificationCode;
            string chars;

            switch (type)
            {
                case CustomerRegistrationVerificationSMSTypes.OnlyNumbers:
                    chars = "0123456789";
                    break;
                case CustomerRegistrationVerificationSMSTypes.OnlyLetters:
                    chars = "abcdefghijklmnopqrstuvwxyz";
                    break;
                case CustomerRegistrationVerificationSMSTypes.NumbersAndLetters:
                    chars = "abcdefghijklmnopqrstuvwxyz01234567890123456789";
                    break;
                default:
                    throw new Exception("Message type is not selected");
            }

            if (!Convert.ToBoolean(_config["TestVersion"]))
            {
                verificationCode = new string(Enumerable.Repeat(chars, verificationCodeLength)
            .Select(s => s[rnd.Next(s.Length)]).ToArray());
                _smsMessagingService.SendMessage(phoneNumber, 0, $"Mekangamya stugich code ` {verificationCode}", (_cacheHelper.GetUser()).userID, 19);
            }
            else
            {
                verificationCode = "12345";
            }
            return verificationCode;
        }

        private CustomerRegistrationProcessResult RegistratePhysicalCustomer(PhysicalCustomer customer)
        {
            var result = new CustomerRegistrationProcessResult();

            if (customer != null && customer.customerNumber != default(ulong))
            {
                if (!_acbaOperationService.CheckCustomerUpdateExpired(customer.customerNumber) || customer.quality.key != 1)
                {
                    this.CustomerData.CustomerNumber = customer.customerNumber;
                    this.CustomerData.CustomerQuality = customer.quality.key;

                    bool hasCustomerOnlineBanking = _xbService.HasCustomerOnlineBanking(customer.customerNumber);

                    if (!hasCustomerOnlineBanking)
                    {
                        string name = Utilities.Utils.ConvertAnsiToUnicode(customer.person.fullName.firstName);
                        string surname = Utilities.Utils.ConvertAnsiToUnicode(customer.person.fullName.lastName);

                        if (customer.quality.key == 1 && customer.customerType.key == 6)
                        {
                            CustomerEmail customerEmail = customer.person.emailList.Find(email => email.emailType.key == 5 && email.priority.key == 1 && email.quality.key == 1);
                            CustomerPhone customerPhone = customer.person.PhoneList.Find(phone => phone.phoneType.key == 1 && phone.phone.countryCode == "+374" && phone.priority.key == 1);

                            if (customerEmail != null && customerPhone != null)
                            {
                                string phoneNumber = customerPhone.phone.countryCode + customerPhone.phone.areaCode + customerPhone.phone.phoneNumber;
                                string emailAddress = customerEmail.email.emailAddress;
                                

                                result.RegistrationResponseData = new Dictionary<string, string>() {
                                    { "phoneNumber", phoneNumber.Substring(0, 6) + new StringBuilder(4).Insert(0, "*", 4).ToString() + phoneNumber.Substring(10, 2)},
                                    { "email", emailAddress.Substring(0, 3) + new String('#', emailAddress.Length - 6) + emailAddress.Substring(emailAddress.Length - 3, 3)},
                                    { "nameSurname", name.Substring(0, 2) + new String('#', name.Length - 2) + " " + surname.Substring(0, 2) + new String('#', surname.Length - 2)},
                                    { "customerNumber", customer.customerNumber.ToString()}
                                };

                                result.RegistrationResult = RegistrationResult.ClientWithCustomerQuality;
                                //result.ResultDescription = "Բանկում գրանցված տվյալները հաճախորդին երևում են ոչ լիարժեք ---> sharunakvum e grancume";

                                //remove the + from +374xxyyyyyy
                                this.CustomerData.PhoneNumber = phoneNumber.Remove(0, 1);
                                this.CustomerData.Email = emailAddress;
                            }
                            else
                            {
                                result.RegistrationResult = RegistrationResult.ClientHavingCustomerQualityWithInsufficientData;
                                result.ResultDescription = "Հարգելի հաճախորդ, Ձեր օնլայն գրանցումը հնարավոր չէ շարունակել: Խնդրում ենք մոտենալ ցանկացած մասնաճյուղ:";
                            }
                        }
                        else
                        {
                            result.RegistrationResult = RegistrationResult.ClientWithNonCustomerQuality;
                            result.RegistrationResponseData = new Dictionary<string, string>() {
                                    { "nameSurname", name.Substring(0, 2) + new String('#', name.Length - 2) + " " + surname.Substring(0, 2) + new String('#', surname.Length - 2)},
                                    { "customerNumber", customer.customerNumber.ToString()}
                                };
                            //result.ResultDescription = "Ցանկացած այլ կարգավիճակ ունեցող հաճախորդ (օր` դիմորդ, ելք կատարող անձ և այլն) կուղղորվի դեպի ոչ հաճախորդ սցենարով և կունենա նոր հաճախորդ համար:";

                        }

                        string registrationToken = this.RegistrateByIdentificationResult(result.RegistrationResult);

                        if (!String.IsNullOrEmpty(registrationToken))
                            result.RegistrationResponseData.Add("RegistrationToken", registrationToken);
                    }
                    else
                    {
                        result.RegistrationResult = RegistrationResult.ExistingOnlineUser;
                        result.ResultDescription = "Դուք արդեն հանդիսանում եք «ACBA Digital» օգտատեր:";
                    }
                }
                else
                {
                    result.RegistrationResult = RegistrationResult.UpdateExpired;
                    result.ResultDescription = "Հարգելի հաճախորդ, Ձեր օնլայն գրանցումը հնարավոր չէ շարունակել: Խնդրում ենք մոտենալ ցանկացած մասնաճյուղ:";
                }
            }
            return result;
        }

        /// <summary>
        /// Կախված իդենտիֆիկացիայի արդյունքից պահպանում է հաճախորդի տվյալները հետագա գրանցումը շարունակելու համար
        /// </summary>
        /// <param name="registrationResult"></param>
        /// <returns>
        /// Վերադարձնում է ունիկալ կոդ, որով հաջորդ հարցումների ժամանակ հնարավոր է ունենալ արդեն պահպանված տվյալները
        /// </returns>
        private string RegistrateByIdentificationResult(RegistrationResult registrationResult)
        {
            string registrationToken = null;
            if (registrationResult == RegistrationResult.ClientWithCustomerQuality || registrationResult == RegistrationResult.ClientWithNonCustomerQuality)
                registrationToken = this.SaveCustomerData();

            return registrationToken;
        }

        /// <summary>
        /// Պահպանում է գրանցվողի սկզբնական տվյալները
        /// </summary>
        /// <returns></returns>
        private string SaveCustomerData()
        {
            string identifier = Guid.NewGuid().ToString();
            _cache.Set("RegistrationCustomerData_" + identifier, this.CustomerData, 15);
            return identifier;
        }

        /// <summary>
        /// Ավելացնում է հեռախոսահամարը հաճախորդի տվյալներ մեջ
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="customerNumber"></param>
        private RegistrationActionResult AddPhoneForCustomer(string phoneNumber, ulong customerNumber)
        {
            var result = new RegistrationActionResult();
            var phone = new CustomerPhone();
            phone.phoneType = new KeyValue();
            phone.phone = new Phone();
            phone.priority = new KeyValue();
            phone.priority.key = 1;
            phone.phoneType.key = 1;
            phone.phone.areaCode = phoneNumber.Substring(3, 2);
            phone.phone.countryCode = "+374";
            phone.phone.phoneNumber = phoneNumber.Substring(5);
            phone.Checked = new KeyValue
            {
                key = 1
            };

            var saveResult = _acbaOperationService.SaveCustomerPhone(customerNumber, phone);
            if (saveResult.resultCode == 1)
                result.ResultCode = CustomerRegistration.ResultCode.Normal;
            else if (saveResult.resultCode == 2)
            {
                throw new Exception(saveResult.resultDescription);
            }

            return result;
        }
        /// <summary>
        /// Ավելացնում է email հաճախորդի տվյալներ մեջ
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="customerNumber"></param>
        private RegistrationActionResult AddEmailForCustomer(string emailAddress, ulong customerNumber)
        {
            var result = new RegistrationActionResult();
            CustomerEmail email = new CustomerEmail
            {
                emailType = new KeyValue(),
                email = new Email(),
                quality = new KeyValue(),
                priority = new KeyValue()
            };
            email.priority.key = 1;
            email.emailType.key = 5;
            email.quality.key = 1;
            email.email.emailAddress = emailAddress;

            var saveResult = _acbaOperationService.SaveCustomerEmail(customerNumber, email);
            if (saveResult.resultCode == 1)
                result.ResultCode = CustomerRegistration.ResultCode.Normal;
            else if (saveResult.resultCode == 2)
            {
                throw new Exception(saveResult.resultDescription);
            }

            return result;
        }
        #endregion
    }
}
