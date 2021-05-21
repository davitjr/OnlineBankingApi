using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACBAServiceReference;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary;
using OnlineBankingLibrary.Models;
using OnlineBankingLibrary.Models.CustomerRegistration;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using static OnlineBankingApi.Enumerations;
using ResponseExtensions = OnlineBankingApi.Models.ResponseExtensions;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly ACBAOperationService _acbaOperationService;
        private CustomerRegistrationManager _registrationManager { get; set; }
        private readonly XBService _xbService;

        public RegistrationController(CustomerRegistrationManager registrationManager, ACBAOperationService acbaOperationService, XBService xbService)
        {
            _acbaOperationService = acbaOperationService;
            _registrationManager = registrationManager;
            _xbService = xbService;
        }

        /// <summary>
        /// Նույնականացնում է և վերադարձնում է հաճախորդի տվյալները (Արագ գրանցման համակարգ)
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCustomerInfoForAuthentication")]
        public IActionResult GetCustomerInfoForAuthentication([FromBody]CustomerAuthenticationRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<CustomerInfoForAuthentication>() { Result = new CustomerInfoForAuthentication() };
                response.ResultCode = ResultCodes.normal;

                //Եթե նշված չէ կամ սխալ է փաստաթղթի տեսակը։
                if (request.DocumentType != DocumentType.IdentifierCard && request.DocumentType != DocumentType.RApassport && request.DocumentType != DocumentType.BiometricPassport)
                {
                    response.ResultCode = ResultCodes.validationError;
                    response.Description = "Incorect request data";
                    response.Result = null;
                    return ResponseExtensions.ToHttpResponse(response);
                }

                CustomerIdentificationResult identificationResult = null;
                var notIdentifiedCustomer = new PhysicalCustomer() { person = new Person() { documentList = new List<CustomerDocument>() } };
                notIdentifiedCustomer.person.documentList.Add(new CustomerDocument() { documentGroup = new KeyValue() { key = 1 }, documentType = new KeyValue() { key = (short)request.DocumentType }, documentNumber = request.DocumentValue });

                try
                {
                    identificationResult = _acbaOperationService.IdentifyCustomer(notIdentifiedCustomer);
                }
                catch
                {
                    response.Result.ProcessResultCode = CustomerAuthenticationResult.NonCustomer;
                    response.Result.TypeOfDocument = CustomerAuthenticationInfoType.Empty;
                    response.Result.ResultDescription = "Հնարավոր չէ կատարել նույնականացում։";
                    return ResponseExtensions.ToHttpResponse(response);
                }

                bool hasCustomerOnlineBanking = _xbService.HasCustomerOnlineBanking(identificationResult.CustomerNumber);

                if (hasCustomerOnlineBanking)
                {
                    response.Result.ProcessResultCode = CustomerAuthenticationResult.CustomerWithOnlineBanking;
                    response.Result.TypeOfDocument = CustomerAuthenticationInfoType.Empty;
                    response.Result.CustomerNumber = identificationResult.CustomerNumber;
                    response.Result.ResultDescription = "Հաճախորդը ունի օնլայն բանկինգ։";
                    return ResponseExtensions.ToHttpResponse(response);
                }

                response.Result.CustomerNumber = identificationResult.CustomerNumber;
                response.Result.ProcessResultCode = CustomerAuthenticationResult.CustomerWithAttachment;
                response.Result.ResultDescription = "Հաճախորդը գտնված է։";

                //Հաճախորդի անձը հաստատող փաստաթղթեր
                var documents = _acbaOperationService.GetCustomerDocumentList((uint)_acbaOperationService.GetIdentityId(identificationResult.CustomerNumber)).FindAll(doc => doc.documentGroup.key == 1);
                documents.Sort((x, y) => y.id.CompareTo(x.id));

                foreach (var document in documents)
                {
                    var attachments = _acbaOperationService.GetAttachmentDocumentList(Convert.ToUInt64(document.id));
                    if (attachments.Count != 0)
                    {
                        attachments.Sort((x, y) => x.PageNumber.CompareTo(y.PageNumber));
                        attachments.ForEach(item =>
                        {
                            response.Result.Data.Add(new KeyValuePair<string, string>(Convert.ToBase64String(_acbaOperationService.GetOneAttachment(item.id)), ((TypeOfAttachments)item.FileExtension).ToString()));
                        });
                        response.Result.TypeOfDocument = CustomerAuthenticationInfoType.Document;
                        break;
                    }
                }

                if (response.Result.Data.Count == 0)
                {
                    response.Result.ProcessResultCode = CustomerAuthenticationResult.CustomerWithNoAttachments;
                    response.Result.TypeOfDocument = CustomerAuthenticationInfoType.Empty;
                    response.Result.ResultDescription = "Առկա չէ հաճախորդին կցված փաստաթուղթ։";
                }

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Կախված փոխանցված արժեքներից իդենտիֆիկացնում է հաճախորդին, և պահպանում գրանցման համար անհրաժեշտ նախնական տվյալները
        /// </summary>
        /// <returns></returns>
        [HttpPost("RegistratePhysicalCustomer")]
        public IActionResult RegistratePhysicalCustomer([FromBody]CustomerRegParams regParams)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<CustomerRegistrationProcessResult>();
                response.ResultCode = ResultCodes.normal;
                _registrationManager.RegParams = regParams;

                switch (regParams.RegType)
                {
                    case TypeOfPhysicalCustomerRegistration.ByPersonalInformation:
                        response.Result = _registrationManager.RegistrateByPersonalInformation();
                        break;
                    case TypeOfPhysicalCustomerRegistration.ByBankProductInformation:
                        response.Result = _registrationManager.RegistrateByBankProductInformation();
                        break;
                }

                //Որպեսզի front-end-ում parse-ի error chta
                if (response.Result.RegistrationResponseData.Count == 0)
                    response.Result.RegistrationResponseData = null;

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        [HttpPost("SendSMSVerificationCodeByRegistrationToken")]
        public IActionResult SendSMSVerificationCodeByRegistrationToken([FromBody]PhoneNumberRequest request, [FromHeader(Name = "RegistrationToken")] string registrationToken)
        {
            if (ModelState.IsValid)
            {
                var response = new Response();
                var data = new RegistrationCustomerData() { RegistrationToken = registrationToken, PhoneNumber = request.PhoneNumber };

                var result = _registrationManager.ContinueRegistrationProcess(data, RegistrationProcessSteps.SendSMSVerificationCodeByRegistrationToken);
                response.ResultCode = ResultCodeFormatter.FromCustomerRegistrationProcess(result.ResultCode);
                response.Description = result.Description;

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        [HttpPost("CheckSMSVerificationCodeByRegistrationToken")]
        public IActionResult CheckSMSVerificationCodeByRegistrationToken([FromBody] VerificationCodeRequest request, [FromHeader(Name = "RegistrationToken")] string registrationToken)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<bool>();
                response.ResultCode = ResultCodes.normal;
                RegistrationCustomerData data;

                if (!string.IsNullOrEmpty(request.VerificationCode))
                {
                    data = new RegistrationCustomerData() { VerificationCode = request.VerificationCode, RegistrationToken = registrationToken };
                    var result = _registrationManager.ContinueRegistrationProcess(data, RegistrationProcessSteps.CheckSMSVerificationCodeByRegistrationToken);

                    response.Description = result.Description;

                    if (result.ResultCode == ResultCode.Normal)
                    {
                        response.Result = true;
                    }
                    else if (result.ResultCode == ResultCode.ValidationError)
                    {
                        response.ResultCode = ResultCodes.validationError;
                    }
                }

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        [HttpPost("GenerateAcbaOnline")]
        public IActionResult GenerateAcbaOnline([FromBody] RegistrationCustomerData data, [FromHeader(Name = "RegistrationToken")] string registrationToken)
        {
            if (ModelState.IsValid)
            {
                var response = new Response();

                data.RegistrationToken = registrationToken;
                if (data.Password != data.RePassword)
                {
                    response.ResultCode = ResultCodes.validationError;
                    response.Description = "Մուտքագրված գաղտնաբառերը չեն համընկնում: Մուտքագրեք ճիշտ գաղտնաբառ:";

                    return ResponseExtensions.ToHttpResponse(response);
                }
                var result = _registrationManager.ContinueRegistrationProcess(data, RegistrationProcessSteps.GenerateAcbaOnline);
                response.ResultCode = ResultCodeFormatter.FromCustomerRegistrationProcess(result.ResultCode);
                response.Description = result.Description;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
    }
}