using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingLibrary.Services;
using XBSInfo;
using XBS;
using static OnlineBankingApi.Enumerations;
using OnlineBankingLibrary.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using Microsoft.AspNetCore.Authorization;
using OnlineBankingLibrary.Utilities;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class InfoController : ControllerBase
    {
        private readonly XBInfoService _xBInfoService;
        private readonly XBService _xBService;
        private readonly CacheHelper _cacheHelper;

        public InfoController(XBInfoService xBInfoService, XBService xBService, CacheHelper cacheHelper)
        {
            _xBInfoService = xBInfoService;
            _xBService = xBService;
            _cacheHelper = cacheHelper;
        }

        /// <summary>
        /// Վերադարձնում է երկրների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCountries")]
        public IActionResult GetCountries()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCountries();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է դեսպանատների ցանկը
        /// </summary>
        /// <param name="referenceTypes"></param>
        /// <returns></returns>
        [HttpPost("GetEmbassyList")]
        public IActionResult GetEmbassyList(ReferenceTypesRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetEmbassyList(request.ReferenceTypes);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է մասնաճյուղերի ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetFilialList")]
        public IActionResult GetFilialList()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetFilialList();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է մասնաճյուղերի ցանկը՝ բացառությամբ Կենտրոն մասնաճյուղի
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetReferenceOrderFilialList")]
        public IActionResult GetReferenceOrderFilialList()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetFilialList();
                response.Result.RemoveAll(m => m.Key == "22000");
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տեղեկանքի լեզուների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetReferenceLanguages")]
        public IActionResult GetReferenceLanguages()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetReferenceLanguages();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տեղեկանքի տեսակների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetReferenceTypes")]
        public IActionResult GetReferenceTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetReferenceTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է վարկային դիմումի գումարի ընտրության ցուցակը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetListOfLoanApplicationAmounts")]
        public IActionResult GetListOfLoanApplicationAmounts()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetListOfLoanApplicationAmounts();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաղորդակցման եղանակների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCommunicationTypes")]
        public IActionResult GetCommunicationTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCommunicationTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է մարզերի ցանկը
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        [HttpPost("GetRegions")]
        public IActionResult GetRegions(CountryRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetRegions(request.Country);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է շրջանը
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        [HttpPost("GetArmenianPlaces")]
        public IActionResult GetArmenianPlaces(RegionRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetArmenianPlaces(request.Region);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ արագ դրամական համակարգով հասանելի արժույթների ցանկը
        /// </summary>
        /// <param name="transferSystem"></param>
        /// <returns></returns>
        [HttpPost("GetTransferSystemCurrency")]
        public IActionResult GetTransferSystemCurrency(TransferSystemRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetTransferSystemCurrency(request.TransferSystem);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է արագ դրամական համակարգերի ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetFastTransferSystemTypes")]
        public IActionResult GetFastTransferSystemTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetFastTransferSystemTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ ավանդատեսակին համապատասխան արժույթների ցանկը
        /// </summary>
        /// <param name="depositType"></param>
        /// <returns></returns>
        [HttpPost("GetDepositTypeCurrencies")]
        public IActionResult GetDepositTypeCurrencies(DepositTypeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetDepositTypeCurrencies(request.DepositType);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է նոր ավանդի հայտի ժամանակ առաջարկվող ավանդատեսակները
        /// </summary>
        /// <param name="accountType"></param>
        /// <param name="customerType"></param>
        /// <returns></returns>
        [HttpPost("GetActiveDepositTypesForNewDepositOrder")]
        public IActionResult GetActiveDepositTypesForNewDepositOrder(ActiveDepositTypesRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetActiveDepositTypesForNewDepositOrder(request.AccountType, request.CustomerType);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է նոր ավանդի հայտի ժամանակ առաջարկվող ավանդատեսակները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetJointTypes")]
        public IActionResult GetJointTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetJointTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քարտի փակման պատճառների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardClosingReasons")]
        public IActionResult GetCardClosingReasons()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCardClosingReasons();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է գումարի ստացման և փոխանցման հայտի արժույթները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCashOrderCurrencies")]
        public IActionResult GetCashOrderCurrencies()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCashOrderCurrencies();

                Dictionary<string, string> result = new Dictionary<string, string>();

                for (int i = 0; i < response.Result.Count; i++)
                {
                    result.Add(response.Result[i].Value, response.Result[i].Value);
                }
                response.Result = result.ToList();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է լիազորագրի գործողության խմբերը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAssigneeOperationGroupTypes")]
        public IActionResult GetAssigneeOperationGroupTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                XBS.AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                response.Result = _xBInfoService.GetAssigneeOperationGroupTypes(authorizedCustomer.TypeOfClient);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ընթացիկ հաշվի բացման հայտի համար հասանելի արժույթները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCurrentAccountOrderCurrencies")]
        public IActionResult GetCurrentAccountOrderCurrencies()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCurrentAccountOrderCurrencies();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քաղվածքի ստացման եղանակները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetStatementDeliveryTypes")]
        public IActionResult GetStatementDeliveryTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetStatementDeliveryTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ավանդի դադարեցման պատճառների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDepositClosingReasonTypes")]
        public IActionResult GetDepositClosingReasonTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetDepositClosingReasonTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քարտի բլոկավորման/ապաբլոկավորման հայտի պատճառները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetReasonsForCardTransactionAction")]
        public IActionResult GetReasonsForCardTransactionAction()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetReasonsForCardTransactionAction();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քարտի բլոկավորման/ապաբլոկավորման գործողության տեսակները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetActionsForCardTransaction")]
        public IActionResult GetActionsForCardTransaction()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetActionsForCardTransaction();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաշվի փակման պատճառների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAccountClosingReasons")]
        public IActionResult GetAccountClosingReasons()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetAccountClosingReasons();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քարտի լիմիտների փոփոխության գործողության տեսակը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardLimitChangeOrderActionTypes")]
        public IActionResult GetCardLimitChangeOrderActionTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCardLimitChangeOrderActionTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Մասնաճյուղի ընտրության հնարավորություն
        /// </summary>
        /// <param name="communalType"></param>
        /// <returns></returns>
        [HttpPost("GetCommunalBranchList")]
        public IActionResult GetCommunalBranchList(CommunalTypesRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCommunalBranchList(request.CommunalType);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հօգուտ 3-րդ անձի փոխանցում կատարելիս պարտատիրոջ կարգավիճակների ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetSyntheticStatuses")]
        public IActionResult GetSyntheticStatuses()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetSyntheticStatuses();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ավանդի գրավով վարկային գծի հայտի 'Պարտադիր մուտքեր' դաշտի ցանկի ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCreditLineMandatoryInstallmentTypes")]
        public IActionResult GetCreditLineMandatoryInstallmentTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCreditLineMandatoryInstallmentTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Փոխանցման եղանակ
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetTransferMethod")]
        public IActionResult GetTransferMethod()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetTransferMethod();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// վերադարձնում է քարտի պատվիրման համար նախատեսված քարտերի համակարգերի ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardSystemTypes")]
        public IActionResult GetCardSystemTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCardSystemTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// վերադարձնում է քարտի պատվիրման համար նախատեսված քարտի ենթատեսակների ցանկը
        /// </summary>
        /// <param name="cardSystem"></param>
        /// <returns></returns>
        [HttpPost("GetCardTypes")]
        public IActionResult GetCardTypes(CardSystemRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCardTypes(request.CardSystem);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// վերադարձնում է քարտի պատվիրման համար առաջարկվող արժույթների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCurrenciesPlasticCardOrder")]
        public IActionResult GetCurrenciesPlasticCardOrder(CurrenciesPlasticCardRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCurrenciesPlasticCardOrder(request.CardType, request.PeriodicityType);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Քաղվածքի ստացման եղանակների ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardReportReceivingTypes")]
        public IActionResult GetCardReportReceivingTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCardReportReceivingTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// PIN-ի ստացման եղանակների ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardPINCodeReceivingTypes")]
        public IActionResult GetCardPINCodeReceivingTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCardPINCodeReceivingTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է արժույթների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCurrencies")]
        public IActionResult GetCurrencies()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCurrencies();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է գործարքների տեսակների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetOrderTypes")]
        public IActionResult GetOrderTypes(GetOrderTypesRequest request)
        {
            SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
            response.ResultCode = ResultCodes.normal;
            response.Result = _xBInfoService.GetDigitalOrderTypes(request != null ? request.HbProductType : TypeOfHbProductTypes.None);
            response.Result.RemoveAll(x => x.Key == "19");
            return ResponseExtensions.ToHttpResponse(response);
        }

        /// <summary>
        /// Վերադարձնում է գործարքների կարգավիճակների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetOrderQualityTypes")]
        public IActionResult GetOrderQualityTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetOrderQualityTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է արժույթների ցանկը արագ դրամական համակարգի համար
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCurrenciesForReceivedFastTransfer")]
        public IActionResult GetCurrenciesForReceivedFastTransfer()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCurrenciesForReceivedFastTransfer();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է պարբերականության տեսակները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPeriodicityTypes")]
        public IActionResult GetPeriodicityTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetPeriodicityTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ՈՒՂԱՐԿՎԱԾ կարգավիճակների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetSentOrderQualityTypes")]
        public IActionResult GetSentOrderQualityTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetSentOrderQualityTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկային գծի տեսակները
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        [HttpPost("GetCreditLineTypesForOnlineMobile")]
        [TypeFilter(typeof(ValidateCardNumberFilter))]
        public IActionResult GetCreditLineTypesForOnlineMobile(CardNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                int cardType = _xBService.GetCardType(request.CardNumber);
                response.Result = _xBInfoService.GetCreditLineTypesForOnlineMobile(cardType);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Քարտի պատվիրման հայտի համար հասանելի քարտատեսակների ցանկի ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPlasticCardOrderCardTypes")]
        public IActionResult GetPlasticCardOrderCardTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBService.GetPlasticCardOrderCardTypes();
                response.Result.Remove(response.Result.First(x => x.Key == "21"));
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ձևանմուշների տեսակների ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetTemplateDocumentTypes")]
        public IActionResult GetTemplateDocumentTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetTemplateDocumentTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Լրացուցիչ քարտի տեսակի ստացում
        /// </summary>
        /// <param name="mainCardNumber"></param>
        /// <returns></returns>
        [HttpPost("GetAttachedCardTypes")]
        public IActionResult GetAttachedCardTypes(MainCardNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetAttachedCardTypes(request.MainCardNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ՏՀՏ կոդերի ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetLTACodes")]
        public IActionResult GetLTACodes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetLTACodes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է լիազորագրի տեսակ
        /// </summary>
        /// <param name="typeOfCustomer"></param>
        /// <param name="customerFilialCode"></param>
        /// <param name="userFilialCode"></param>
        /// <returns></returns>
        [HttpPost("GetCredentialTypes")]
        public IActionResult GetCredentialTypes(CredentialTypesRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetCredentialTypes(request.TypeOfCustomer, request.CustomerFilialCode, request.UserFilialCode);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի ԴԱՀԿ սառեցումները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDAHKFreezings")]
        public IActionResult GetDAHKFreezings()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<DAHKFreezing>> response = new SingleResponse<List<DAHKFreezing>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBService.GetDAHKFreezings();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է լիազորագրի գործողության խմբի գործողության տեսակները
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPost("GetAssigneeOperationTypes")]
        public IActionResult GetAssigneeOperationTypes(GroupIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Tuple<int, int, string, bool>>> response = new SingleResponse<List<Tuple<int, int, string, bool>>>();


                XBS.AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                List<Tuple<int, int, string, bool>> operationTypesList = _xBInfoService.GetAssigneeOperationTypes(request.GroupId, authorizedCustomer.TypeOfClient);

                List<Tuple<int, int, string, bool>> operationTypesListNew = new List<Tuple<int, int, string, bool>>();

                foreach (var type in operationTypesList)
                {

                    if (type.Item2 != 14 && (type.Item2 == 16 || type.Item2 == 17))
                    {

                        operationTypesListNew.Add(new Tuple<int, int, string, bool>(type.Item1, type.Item2, type.Item3, false));

                    }
                    else
                    {
                        operationTypesListNew.Add(new Tuple<int, int, string, bool>(type.Item1, type.Item2, type.Item3, type.Item4));

                    }
                }

                response.Result = operationTypesListNew;
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հաջորդ գործառնական օր
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetNextOperDay")]
        public IActionResult GetNextOperDay()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<DateTime> response = new SingleResponse<DateTime>();
                response.Result = _xBService.GetNextOperDay();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է արագ օվերդրաֆտի սկզբի և վերջի ամսաթվերը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetFastOverdrafStartAndEndDate")]
        public IActionResult GetFastOverdrafStartAndEndDate()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<FastOverdraftDates> response = new SingleResponse<FastOverdraftDates>();
                DateTime start = _xBService.GetNextOperDay();
                response.Result = _xBInfoService.GetFastOverdrafStartAndEndDate(start);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ԿԲ փոխարժեքների ընթացիկ տվյալները
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpPost("GetCBKursForDate")]
        public IActionResult GetCBKursForDate(CBKursForDateRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.Result = _xBService.GetCBKursForDate(request.Date, request.Currency);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է Արագ օվերդրաֆտի միջնորդավճարի գումարը
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        [HttpPost("GetFastOverdraftFeeAmount")]
        public IActionResult GetFastOverdraftFeeAmount(AmountRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.Result = _xBInfoService.GetFastOverdraftFeeAmount(request.Amount);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ բիզնես ավանդի օպցիանրին համապատասխան տոկոսադրույքը
        /// </summary>
        /// <param name="depositOption"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        [HttpPost("GetBusinesDepositOptionRate")]
        public IActionResult GetBusinesDepositOptionRate(BusinesDepositOptionRateRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<object> response = new SingleResponse<object>();
                response.Result = _xBService.GetBusinesDepositOptionRate(request.DepositOption, request.Currency);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Առաջարկվող գաղտնաբառի ստացում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        [HttpPost("GetNewCardPassword")]
        public IActionResult GetNewCardPassword()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                XBS.AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                response.Result = _xBInfoService.GetNewCardPassword(authorizedCustomer.CustomerNumber);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է փոխարժեքի ընթացիկ տվյալները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetLastExchangeRate")]
        public IActionResult GetLastExchangeRate(LastExchangeRateRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.Result = _xBService.GetLastExchangeRate(request.Currency, request.RateType, request.Direction);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է գումարի ստացման և փոխանցման հայտի տեսակները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCashOrderTypes")]
        public IActionResult GetCashOrderTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.Result = _xBInfoService.GetCashOrderTypes();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է լիազորագրի հայտի մուտքագրման միջնորդավճարը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCredentialOrderFee")]
        public IActionResult GetCredentialOrderFee()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.Result = _xBService.GetCredentialOrderFee();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի տվյալների փոփոխման հայտում առաջարկվող գաղտնաբարը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPasswordForCustomerDataOrder")]
        public IActionResult GetPasswordForCustomerDataOrder()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.Result = _xBService.GetPasswordForCustomerDataOrder();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի տվյալների փոփոխման հայտում առաջարկվող էլ. հասցեն 
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetEmailForCustomerDataOrder")]
        public IActionResult GetEmailForCustomerDataOrder()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.Result = _xBService.GetEmailForCustomerDataOrder();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է բիզնես ավանդի համար տոկոսադրույքի կարգավորումների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetBusinesDepositOptions")]
        public IActionResult GetBusinesDepositOptions()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<XBSInfo.DepositOption>> response = new SingleResponse<List<XBSInfo.DepositOption>>();
                response.Result = _xBInfoService.GetBusinesDepositOptions();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք տվյալ ամսաթիվը աշխատանքային օր է, թե ոչ
        /// </summary>
        /// <param name="dateWorkingDay"></param>
        /// <returns></returns>
        [HttpPost("IsWorkingDay")]
        public IActionResult IsWorkingDay(DateWorkingDayRequest request)
        {
            SingleResponse<bool> response = new SingleResponse<bool>();
            if (request != null)
            {
                response.Result = _xBInfoService.IsWorkingDay(request.DateWorkingDay);
                response.ResultCode = ResultCodes.normal;
            }
            else
            {
                response.ResultCode = ResultCodes.failed;
                response.Description = "Ամսաթվի արժեքը սխալ է։";
            }
            return ResponseExtensions.ToHttpResponse(response);
        }

        /// <summary>
        /// «Գրավադրվող գումարի արժույթ» կամ «Պարտադիր մուտքեր» ինֆորմացիա
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetMandatoryEntryInfo")]
        public IActionResult GetMandatoryEntryInfo(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();

                string MandatoryEntryInfo = _xBInfoService.GetMandatoryEntryInfo((byte)request.Id);


                if (!String.IsNullOrEmpty(MandatoryEntryInfo))
                {
                    response.Result = MandatoryEntryInfo.Replace("§", "«").Replace("¦", "»");
                    response.ResultCode = ResultCodes.normal;
                }
                else
                {
                    response.ResultCode = ResultCodes.failed;
                    response.Description = "Error!Please try later";
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ Բանկի անվանումը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetBankName")]
        public IActionResult GetBankName(CodeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();

                if (request.Code >= 90000 && request.Code <= 90048)
                {
                    request.Code = 10300;
                }
                response.Result = _xBInfoService.GetBankName(request.Code);
                if (request.Code.ToString()[0] == '9' && !String.IsNullOrEmpty(response.Result))
                {
                    response.Result = _xBInfoService.GetBankName(10300);
                }

                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է բոլոր արժույթների կուրսերը լիստով
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetExchangeRates")]
        public IActionResult GetExchangeRates()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<ExchangeRate>> response = new SingleResponse<List<ExchangeRate>>();
                response.Result = _xBService.GetExchangeRates();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քարտի պայմանները
        /// </summary>
        /// <param name="cardType"></param>
        /// <param name="periodicityType"></param>
        /// <returns></returns>
        [HttpPost("GetCardTariffsByCardType")]
        public IActionResult GetCardTariffsByCardType(CardTariffsByCardTypeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<XBSInfo.CardTariff> response = new SingleResponse<XBSInfo.CardTariff>();
                XBSInfo.CardTariffContract contract = _xBInfoService.GetCardTariffsByCardType(request.CardType, request.PeriodicityType);
                if (contract != null & contract.CardTariffs != null && contract.CardTariffs.Count > 0)
                {
                    response.Result = contract.CardTariffs.Find(t => t.Currency == request.Currency);
                }

                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }

        }

        /// <summary>
        /// Վերադարձնում է քարտի միջնորդավճարը
        /// </summary>
        /// <param name="cardType"></param>
        /// <param name="officeId"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        [HttpPost("GetCardServiceFee")]
        public IActionResult GetCardServiceFee(CardServiceFeeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.Result = _xBInfoService.GetCardServiceFee(request.CardType, request.OfficeId, request.Currency);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ավանդի սակագները ըստ տեսակի
        /// </summary>
        /// <param name="depositType"></param>
        /// <returns></returns>
        [HttpPost("GetDepositRateTariff")]
        public IActionResult GetDepositRateTariff(DepositTypeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<DepositRateTariff> response = new SingleResponse<DepositRateTariff>();
                response.Result = _xBService.GetDepositRateTariff((XBS.DepositType)request.DepositType);
                response.Result.DepositRateTariffItems.RemoveAll(p => p.InterestRate == 0);
                foreach (var item in response.Result.DepositRateTariffItems)
                {
                    item.InterestRate += item.BonusInterestRateForHB;
                    item.InterestRate = Convert.ToDouble(String.Format("{0:0.0000}", item.InterestRate));
                    
                }
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք տվյալ հաշվեհամարը ոստիկանության հաշիվ է, թե ոչ
        /// </summary>
        /// <param name="request">Հաշվեհամար</param>
        /// <returns></returns>
        [HttpPost("IsPoliceAccount")]
        public IActionResult IsPoliceAccount(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<bool> response = new SingleResponse<bool>();
                response.Result = _xBService.IsPoliceAccount(request.AccountNumber);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ոստիկանության կոդերի ցանկը
        /// </summary>
        /// <param name="request">Ոստիկանության հաշվեհամար</param>
        /// <returns></returns>
        [HttpPost("GetPoliceCodes")]
        public IActionResult GetPoliceCodes(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetPoliceCodes(request.AccountNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է աբոնենտի տեսակները կոմունալ վճարման համար
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetUtilityAbonentTypes")]
        public IActionResult GetUtilityAbonentTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<byte, string>>> response = new SingleResponse<List<KeyValuePair<byte, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetUtilityAbonentTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է Online Banking-ում հասանելի կոմունալ վճարումների տեսակների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetUtilityPaymentTypes")]
        public IActionResult GetUtilityPaymentTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<short, string>>> response = new SingleResponse<List<KeyValuePair<short, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetUtilityPaymentTypesOnlineBanking();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է կոմունալ վճարման պարբերականի դեպքում վճարման ենթատեսակը (ԳազՊրոմի դեպքում)
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPeriodicsSubTypes")]
        public IActionResult GetPeriodicsSubTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetPeriodicsSubTypes();
                response.Result.RemoveAll(x => x.Key.Equals("1"));
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է կոմունալի պարբերականների դեպքում վճարման տեսակների ցանկը (Պարտքի առկայության դեպքում/Անկախ պարտքի առկայությունից)
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPayIfNoDebtTypes")]
        public IActionResult GetPayIfNoDebtTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<short, string>>> response = new SingleResponse<List<KeyValuePair<short, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetPayIfNoDebtTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// վերադարձնում է քարտի սպասարկման վարձի պարբերականության տեսակները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetServiceFeePeriodocityTypes")]
        public IActionResult GetServiceFeePeriodocityTypes()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<int, string>>> response = new SingleResponse<List<KeyValuePair<int, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetServiceFeePeriodocityTypes();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է Հաճախորդի գրանցված մասնաճյուղը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCustomerFilial")]
        public IActionResult GetCustomerFilial()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<short> response = new SingleResponse<short>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBService.GetCustomerFilial();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է կից քարտի միջնորդավճարի ցանկը
        /// </summary>
        /// <param name="cardType"></param>
        /// <returns></returns>
        [HttpPost("GetLinkedCardTariffsByCardType")]
        public IActionResult GetLinkedCardTariffsByCardType(CardTypeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetLinkedCardTariffsByCardType(request.CardType);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// վերադարձնում է լրացուցիչ քարտի պայմանները
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetAttachedCardTariffs")]
        public IActionResult GetAttachedCardTariffs(AttachedCardTariffsRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<XBSInfo.CardTariff> response = new SingleResponse<XBSInfo.CardTariff>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetAttachedCardTariffs(request.MainCardNumber, request.cardType);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է վարկի մարման տեսակները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetLoanMatureTypesForIBanking")]
        public IActionResult GetLoanMatureTypesForIBanking()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.Result = _xBInfoService.GetLoanMatureTypesForIBanking();
                response.Result.RemoveAll(x => x.Key == "4");
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Վերադարձնում է պլաստիկ քարտի SMS սերվիսի գործողության տեսակը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPlasticCardSmsServiceActions")]
        public IActionResult GetPlasticCardSmsServiceActions(CardNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.Result = _xBInfoService.GetPlasticCardSmsServiceActions(request.CardNumber);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է պլաստիկ քարտի SMS ծառայության տեսակը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetTypeOfPlasticCardsSMS")]
        public IActionResult GetTypeOfPlasticCardsSMS()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.Result = _xBInfoService.GetTypeOfPlasticCardsSMS();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է փոխանցման եղանակների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDetailsOfCharges")]
        public IActionResult GetDetailsOfCharges()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.Result = _xBInfoService.GetDetailsOfCharges();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// վերադարձնում է ծառայության միջնորդավճարի գումարը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetServiceProvidedOrderFee")]
        public IActionResult GetServiceProvidedOrderFee(IndexRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.Result = _xBService.GetServiceProvidedOrderFee(request.Index);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// վերադարձնում է "տվյալների փոփոխման հայտ"-ի համար անհրաժեշտ տվյալներ
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetInfoForCustomerDataChangeOrder")]
        public IActionResult GetInfoForCustomerDataChangeOrder()
        {
            SingleResponse<CustomerData> response = new SingleResponse<CustomerData>();
            var mainData = _xBService.GetCustomerMainData(_cacheHelper.GetAuthorizedCustomer().CustomerNumber);
            var mainEmail = mainData.Emails.Count != 0 ? mainData.Emails.First().email.emailAddress : "";
            var HomePhone = mainData.Phones.Where(x => x.phoneType.key != 1).Count() != 0 ? mainData.Phones.Where(x => x.phoneType.key != 1).First().phone : new Phone();
            var mainMobilePhone = mainData.Phones.Where(x => x.phoneType.key == 1).Count() != 0 ? mainData.Phones.Where(x => x.phoneType.key == 1).First().phone : new Phone();
            response.Result = new CustomerData();
            response.Result.MainEmail = mainEmail;
            response.Result.MainMobilePhone = mainMobilePhone.countryCode + mainMobilePhone.areaCode + mainMobilePhone.phoneNumber;
            response.Result.HomePhone = HomePhone.countryCode + HomePhone.areaCode + HomePhone.phoneNumber;
            response.Result.Password = _xBService.GetPasswordForCustomerDataOrder() ?? "";
            response.ResultCode = ResultCodes.normal;
            return ResponseExtensions.ToHttpResponse(response);
        }

        /// <summary>
        /// վերադարձնում է գործառնական օրը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCurrentOperDay")]
        public IActionResult GetCurrentOperDay()
        {
            SingleResponse<DateTime> response = new SingleResponse<DateTime>();
            response.Result = _xBService.GetCurrentOperDay();
            response.ResultCode = ResultCodes.normal;
            return ResponseExtensions.ToHttpResponse(response);
        }

        /// <summary>
        /// Վերադարձնում է Քաղվածքի էլեկտրոնային ստացման դիմումի պարբերականությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetStatementFrequency")]
        public IActionResult GetStatementFrequency()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetStatementFrequency();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// վերադարձնում է միջազգային փոխանցման արժույթները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetInternationalPaymentCurrencies")]
        public IActionResult GetInternationalPaymentCurrencies()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBInfoService.GetInternationalPaymentCurrencies();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է RUR միջացզային փախանցման տեսակները։
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPaymentTypesForRUR")]
        public IActionResult GetPaymentTypesForRUR()
        {
            SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
            response.Result = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Материальная помощь", "Материальная помощь"),
                new KeyValuePair<string, string>("Оплата за", "Оплата за"),
                new KeyValuePair<string, string>("Предоплата за", "Предоплата за"),
                new KeyValuePair<string, string>("Другое", "Другое")
            };

            response.ResultCode = ResultCodes.normal;
            return ResponseExtensions.ToHttpResponse(response);
        }

        /// <summary>
        /// վերադարձնում է ԱԱՀ-ի տեսակը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetVATForRUR")]
        public IActionResult GetVATForRUR()
        {
            SingleResponse<List<KeyValuePair<string, string>>> response = new SingleResponse<List<KeyValuePair<string, string>>>();
            response.Result = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("без НДС", "без НДС"),
                new KeyValuePair<string, string>("с НДС", "с НДС")
            };

            response.ResultCode = ResultCodes.normal;
            return ResponseExtensions.ToHttpResponse(response);
        }

        /// <summary>
        /// Վերադարձնում է RUR փոխանցման ստացողի տեսակները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetReceicerTypesForRUR")]
        public IActionResult GetReceicerTypesForRUR()
        {
            SingleResponse<List<KeyValuePair<byte, string>>> response = new SingleResponse<List<KeyValuePair<byte, string>>>();
            response.Result = _xBInfoService.GetTransferReceiverTypes();
            response.ResultCode = ResultCodes.normal;
            return ResponseExtensions.ToHttpResponse(response);
        }

        /// <summary>
        /// Ստուգում է, թե ուն՞ի տվյալ հաճախորդը Sap - ում քաղվածքի ստացման նշված եղանակ թե ոչ
        /// </summary>
        /// <returns></returns>
        [HttpPost("CommunicationTypeExistence")]
        public IActionResult CommunicationTypeExistence()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<byte> response = new SingleResponse<byte>();
                XBS.AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                response.Result = _xBInfoService.CommunicationTypeExistence(authorizedCustomer.CustomerNumber);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }




    }
}