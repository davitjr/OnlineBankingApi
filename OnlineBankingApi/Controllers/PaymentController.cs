using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using XBS;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class PaymentController : ControllerBase
    {
        private readonly XBService _xBService;
        private readonly CacheHelper _cacheHelper;
        private readonly IConfiguration _config;
        public PaymentController(XBService xBService, CacheHelper cacheHelper,IConfiguration config)
        {
            _xBService = xBService;
            _cacheHelper = cacheHelper;
            _config = config;
        }

        /// <summary>
        /// Պահպանում է կոմունալ վճարման հանձնարարականը
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost("SaveUtilityPaymentOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveUtilityPaymentOrder(UtilityPaymentOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xBService.SaveUtilityPaymentOrder(request.Order);
                response.Result = result.Id;
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                response.Description = Utils.GetActionResultErrors(result.Errors);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է կոմունալ վճարման հանձնարարականի տվյալները
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetUtilityPaymentOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetUtilityPaymentOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xBService.GetUtilityPaymentOrder(request.Id);
                SingleResponse<UtilityPaymentOrder> response = new SingleResponse<UtilityPaymentOrder>();
                response.ResultCode = ResultCodes.normal;
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    if (!_xBService.HasProductPermission(order.DebitAccount.AccountNumber))
                    {
                        hasPermission = false;
                        response.ResultCode = ResultCodes.failed;
                        response.Result.Description = "Տվյալները հասանելի չեն։";
                    }
                }

                if (hasPermission)
                {
                    if (order.Quality == OrderQuality.Declined)
                    {
                        order.RejectReason = _xBService.GetOrderRejectReason(request.Id, order.Type);
                    }
                    response.Result = order;
                    response.ResultCode = ResultCodes.normal;
                }

                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հաստատում/ուղարկում է կոմունալ վճարման հայտը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("ApproveUtilityPaymentOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.UtilityPaymentOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveUtilityPaymentOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                UtilityPaymentOrder order = _cacheHelper.GetApprovalOrder<UtilityPaymentOrder>(request.Id);
                XBS.ActionResult saveResult = _xBService.ApproveUtilityPaymentOrder(order);

                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(saveResult.ResultCode);
                response.Result = saveResult.Id;
                response.Description = Utils.GetActionResultErrors(saveResult.Errors);

                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ՃՈ խախտումները որոշման համարով
        /// </summary>
        /// <param name="violationId"></param>
        /// <returns></returns>
        [HttpPost("GetVehicleViolationById")]
        public IActionResult GetVehicleViolationById(ViolationIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<VehicleViolationResponse>> response = new SingleResponse<List<VehicleViolationResponse>>();
                if (Convert.ToBoolean(_config["TestVersion"]))
                {
                    VehicleViolationResponse item1 = new VehicleViolationResponse();
                    item1.Id = 176709;
                    item1.FineAmount = 2000;
                    item1.PayableAmount = 2000;
                    item1.PenaltyAmount = 0;
                    item1.PayedAmount = 0;
                    item1.PoliceAccount = "900013150058";
                    item1.RequestedAmount = 2000;
                    item1.ResponseId = 166716;
                    item1.VehicleModel = "KIA RIO 1.4";
                    item1.VehicleNumber = "133OU64";
                    item1.VehiclePassport = "SC067423";
                    item1.ViolationDate = Convert.ToDateTime("2019-04-04 12:17:23.000");
                    item1.ViolationNumber = request.ViolationId;
                    response.Result = new List<VehicleViolationResponse>();
                    response.Result.Add(item1);
                }
                else
                    response.Result = _xBService.GetVehicleViolationById(request.ViolationId);
               
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ՃՈ խախտումները մեքենայի հաշվառման համարանիշով և տեխ. անձնագրի համարով
        /// </summary>
        /// <param name="psn"></param>
        /// <param name="vehNum"></param>
        /// <returns></returns>
        [HttpPost("GetVehicleViolationByPsnVehNum")]
        public IActionResult GetVehicleViolationByPsnVehNum(VehicleViolationByPsnVehRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<VehicleViolationResponse>> response = new SingleResponse<List<VehicleViolationResponse>>();
                if (Convert.ToBoolean(_config["TestVersion"]))
                {
                    VehicleViolationResponse item1 = new VehicleViolationResponse();
                    VehicleViolationResponse item2 = new VehicleViolationResponse();
                    item1.Id = 176709;
                    item1.FineAmount = 2000;
                    item1.PayableAmount = 2000;
                    item1.PenaltyAmount = 0;
                    item1.PayedAmount = 0;
                    item1.PoliceAccount = "900013150058";
                    item1.RequestedAmount = 2000;
                    item1.ResponseId = 166716;
                    item1.VehicleModel = "KIA RIO 1.4";
                    item1.VehicleNumber = request.VehNum;
                    item1.VehiclePassport = request.Psn;
                    item1.ViolationDate = Convert.ToDateTime("2019-04-04 12:17:23.000");
                    item1.ViolationNumber = "1909388733";

                    item2.Id = 176708;
                    item2.FineAmount = 4000;
                    item2.PayableAmount = 4000;
                    item2.PenaltyAmount = 0;
                    item2.PayedAmount = 0;
                    item2.PoliceAccount = "900013150058";
                    item2.RequestedAmount = 4000;
                    item2.ResponseId = 166715;
                    item2.VehicleModel = "LEXUS GX 460";
                    item2.VehicleNumber = request.VehNum;
                    item2.VehiclePassport = request.Psn;
                    item2.ViolationDate = Convert.ToDateTime("2019-04-05 19:15:13.000");
                    item2.ViolationNumber = "1909395132";
                    response.Result = new List<VehicleViolationResponse>();
                    response.Result.Add(item1);
                    response.Result.Add(item2);
                }
                else
                    response.Result = _xBService.GetVehicleViolationByPsnVehNum(request.Psn, request.VehNum);
                
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է համապատասխան կոմունալ վճարումների ցանկ
        /// </summary>
        /// <param name="searchCommunal"></param>
        /// <returns></returns>
        [HttpPost("GetCommunals")]
        public IActionResult GetCommunals(SearchCommunalRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Communal>> response = new SingleResponse<List<Communal>>();
                if (request.SearchCommunal.CommunalType == XBS.CommunalTypes.ArmWater)
                {
                    response.ResultCode = ResultCodes.failed;
                    //language is needed
                    string errorMessage = "Հարգելի հաճախորդ, «ՀայՋրմուղԿոյուղի» ծառայության պարտքը հարկավոր է վճարել «Երևան Ջուր» բաժնում";
                    response.Description = errorMessage;
                    return ResponseExtensions.ToHttpResponse(response);
                }
                response.Result = _xBService.GetCommunals(request.SearchCommunal);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է համապատասխան կոմունալ վճարման մանրամասները
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetCommunalDetails")]
        public IActionResult GetCommunalDetails(CommunalDetailsRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<CommunalDetails>> response = new SingleResponse<List<CommunalDetails>>();
                response.Result = _xBService.GetCommunalDetails(request.CommunalType, request.AbonentNumber, request.CheckType, request.BranchCode, request.AbonentType);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հեռացնում է Փոխանցում ՀՀ տարածքում/Փոխանցում սեփական հաշիվների միջև հայտը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("DeletePaymentOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult DeletePaymentOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xBService.DeletePaymentOrder(request.Id);

                response.Result = result.Id;
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                response.Description = Utils.GetActionResultErrors(result.Errors);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է արագ դրամական համակարգերով ստացված փոխանցման հայտի տվյալները
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetReceivedFastTransferPaymentOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetReceivedFastTransferPaymentOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<ReceivedFastTransferPaymentOrder> response = new SingleResponse<ReceivedFastTransferPaymentOrder>
                {
                    Result = _xBService.GetReceivedFastTransferPaymentOrder(request.Id)
                };  
                //քարտային հաշիվների դեպքում
                if (response.Result.ReceiverAccount.AccountType == 11)
                {
                    response.Result.ReceiverAccount.ArcaBalance = _xBService.GetArcaBalance(response.Result.ReceiverAccount.ProductNumber
                                .Substring(0, 16)
                                .Trim()); 
                    if(response.Result.ReceiverAccount.AccountNumber == "220000295380000" && Convert.ToBoolean(_config["TestVersion"]))
                        response.Result.ReceiverAccount.ArcaBalance = null;
                }
                response.ResultCode = ResultCodes.normal;
                if (response.Result.Quality == OrderQuality.Declined)
                {
                    response.Result.RejectReason = _xBService.GetOrderRejectReason(request.Id, response.Result.Type);
                }
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Հաստատում/ուղարկում է արագ դրամական համակարգերով փոխանցման ստացման հայտը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("ApproveReceivedFastTransferPaymentOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.ReceivedFastTransferPaymentOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveReceivedFastTransferPaymentOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                ReceivedFastTransferPaymentOrder order = _cacheHelper.GetApprovalOrder<ReceivedFastTransferPaymentOrder>(request.Id);
                XBS.ActionResult result = _xBService.ApproveReceivedFastTransferPaymentOrder(order);
                response.Result = result.Id;
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                response.Description = Utils.GetActionResultErrors(result.Errors);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հայտի համար անհրաժեշտ ավտոմատ դաշտերի ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCustomerDateForInternationalPayment")]
        public IActionResult GetCustomerDateForInternationalPayment()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<InternationalPaymentOrder> response = new SingleResponse<InternationalPaymentOrder>();
                response.Result = _xBService.GetCustomerDateForInternationalPayment();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Բջջային համարի որոնում՝ առանց օպերատորի նշման
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetCommunalsByPhoneNumber")]
        public IActionResult GetCommunalsByPhoneNumber(SearchCommunalRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Communal>> response = new SingleResponse<List<Communal>>();
                string validationResult = _xBService.ValidateSearchData(request.SearchCommunal);
                if (validationResult == "")
                    response.Result = _xBService.GetCommunalsByPhoneNumber(request.SearchCommunal);
                else
                {
                    XBS.ActionResult result = new XBS.ActionResult
                    {
                        ResultCode = ResultCode.ValidationError,
                        Errors = new List<ActionError>()
                    };

                    ActionError actionError = new ActionError();
                    actionError.Description = validationResult;

                    result.Errors.Add(actionError);
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
        /// Պահպանում է արագ դրամական համակարգերով փոխանցման ստացման հայտը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SaveReceivedFastTransferPaymentOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                request.Order.Quality = OrderQuality.Draft;
                request.Order.FilialCode = 22000;
                request.Order.Description = "Non-commercial transfer for personal needs";
                XBS.ActionResult result = _xBService.SaveReceivedFastTransferPaymentOrder(request.Order);
                response.Result = result.Id;
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                response.Description = Utils.GetActionResultErrors(result.Errors);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ արժույթների զույգից cross փոխարժեքի որոշման եղանակը
        /// (ցույց է տալիս թե արժույթներից որը մյուսի վրա պետք է բաժանվի)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetCrossConvertationVariant")]
        public IActionResult GetCrossConvertationVariant(CrossConvertationVariantRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<ushort> response = new SingleResponse<ushort>();
                response.Result = _xBService.GetCrossConvertationVariant(request.DebitCurrency, request.CreditCurrency);
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