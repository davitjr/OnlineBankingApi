using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using System.Linq;
using XBS;
using static OnlineBankingApi.Enumerations;
using ActionResult = XBS.ActionResult;
using ResponseExtensions = OnlineBankingApi.Models.ResponseExtensions;
using utils = OnlineBankingLibrary.Utilities.Utils;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class TransferController : ControllerBase
    {
        private readonly XBService _xBService;
        private readonly CacheHelper _cacheHelper;

        public TransferController(XBService xBService, ReportService reportService, CacheHelper cacheHelper)
        {
            _xBService = xBService;
            _cacheHelper = cacheHelper;
        }


        /// <summary>
        /// Պահպանում է վճարման հանձնարարականը (Փոխանցում ՀՀ տարածքում, սեփական հաշիվների միջև
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost("SavePaymentOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SavePaymentOrder(PaymentOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xBService.SavePaymentOrder(request.Order);
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
        /// Վերադարձնում է Փոխանցում ՀՀ տարածքում/սեփական հաշիվների միջև գործարքի տվյալները
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetPaymentOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetPaymentOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                SingleResponse<PaymentOrder> response = new SingleResponse<PaymentOrder>();
                var order = _xBService.GetPaymentOrder(request.Id);
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    if (!_xBService.HasProductPermission(order.DebitAccount.AccountNumber) || (order.SubType == 3 && !_xBService.HasProductPermission(order.ReceiverAccount.AccountNumber))
                        || (order.FeeAccount != null && order.FeeAccount.AccountNumber != "0" && !_xBService.HasProductPermission(order.FeeAccount.AccountNumber)))
                    {
                        hasPermission = false;
                        response.ResultCode = ResultCodes.failed;
                        response.Description = "Տվյալները հասանելի չեն։";
                    }
                }
                if (hasPermission)
                {
                    if (order.Quality == OrderQuality.Declined)
                    {
                        order.RejectReason = _xBService.GetOrderRejectReason(request.Id, order.Type);
                    }
                    response.ResultCode = ResultCodes.normal;
                    response.Result = order;
                }
                if (response.Result.ReceiverAccount.AccountNumber.StartsWith("103009") && response.Result.ReceiverAccount.AccountNumber.Count() == 17)
                    response.Result.ReceiverAccount.AccountNumber = response.Result.ReceiverAccount.AccountNumber.Substring(5);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հաստատում/ուղարկում է Փոխանցում ՀՀ տարածքում/Փոխանցում սեփական հաշիվների միջև հայտը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("ApprovePaymentOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.PaymentOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApprovePaymentOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                PaymentOrder order = _cacheHelper.GetApprovalOrder<PaymentOrder>(request.Id);
                XBS.ActionResult saveResult = _xBService.ApprovePaymentOrder(order);

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
        /// Վերադարձնում է միջազգային վճարման հանձնարարականի տվյալները
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetInternationalPaymentOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetInternationalPaymentOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xBService.GetInternationalPaymentOrder(request.Id);
                SingleResponse<InternationalPaymentOrder> response = new SingleResponse<InternationalPaymentOrder>();
                response.ResultCode = ResultCodes.normal;
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    if (!_xBService.HasProductPermission(order.DebitAccount.AccountNumber) || (order.SubType == 3 && !_xBService.HasProductPermission(order.ReceiverAccount.AccountNumber))
                        || (order.FeeAccount != null && order.FeeAccount.AccountNumber != "0" && !_xBService.HasProductPermission(order.FeeAccount.AccountNumber)))
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
        /// Միջազգային փոխանցման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost("SaveInternationalPaymentOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveInternationalPaymentOrder(InternationalPaymentOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xBService.SaveInternationalPaymentOrder(request.Order);
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
        /// Ուղարկում է միջազգային փոխանցման հայտը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("ApproveInternationalPaymentOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.InternationalPaymentOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveInternationalPaymentOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                InternationalPaymentOrder order = _cacheHelper.GetApprovalOrder<InternationalPaymentOrder>(request.Id);
                XBS.ActionResult saveResult = _xBService.ApproveInternationalPaymentOrder(order);

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
        /// Վերադարձնում է տվյալ բյուջե փոխանցման հայտի տվյալները
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetBudgetPaymentOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetBudgetPaymentOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xBService.GetBudgetPaymentOrder(request.Id);
                order.ReceiverAccount.AccountNumber = order.ReceiverBankCode + order.ReceiverAccount.AccountNumber;
                SingleResponse<BudgetPaymentOrder> response = new SingleResponse<BudgetPaymentOrder>();
                response.ResultCode = ResultCodes.normal;
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    if (!_xBService.HasProductPermission(order.DebitAccount.AccountNumber) || (order.FeeAccount != null && order.FeeAccount.AccountNumber != "0" && !_xBService.HasProductPermission(order.FeeAccount.AccountNumber)))
                    {
                        hasPermission = false;
                        response.ResultCode = ResultCodes.failed;
                        response.Result.Description = "Տվյալները հասանելի չեն։";
                    }
                }
                if (hasPermission)
                {
                    response.ResultCode = ResultCodes.normal;
                    response.Result = order;
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Պահպանում է բյուջե փոխանցման վճարման հանձնարարականը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveBudgetPaymentOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveBudgetPaymentOrder(BudgetPaymentOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };
                if (request.Order.ReceiverAccount.AccountNumber.StartsWith("10300"))
                {
                    request.Order.ReceiverBankCode = 10300; // ոստիկանության բանկի կոդ
                    request.Order.ReceiverAccount.AccountNumber = request.Order.ReceiverAccount.AccountNumber.Substring(5);
                }
                ActionResult saveResult = _xBService.SaveBudgetPaymentOrder(request.Order);
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(saveResult.ResultCode);
                response.Result = saveResult.Id;
                response.Description = utils.GetActionResultErrors(saveResult.Errors);

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }

        }

        /// <summary>
        /// Վերադարձնում է միջազգային փոխանցման միջնորդավճարը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetInternationalPaymentOrderFee")]
        public IActionResult GetInternationalPaymentOrderFee(InternationalPaymentOrderFeeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBService.GetInternationalPaymentOrderFee(request.Order);
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
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost("GetReceivedFastTransferOrderRejectReason")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetReceivedFastTransferOrderRejectReason(OrderIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBService.GetReceivedFastTransferOrderRejectReason((int)request.OrderId);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// XML ֆայլի կարդացում և գործարքների պահպանում
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpPost("ReadXmlFile")]
        public IActionResult ReadXmlFile(FileIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<ReadXmlFileAndLog> response = new SingleResponse<ReadXmlFileAndLog>();
                response.Result = _xBService.ReadXmlFile(request.FileId);
                foreach (var item in response.Result.paymentOrders)
                {
                    item.ReceiverAccount.AccountNumber = item.ReceiverBankCode + item.ReceiverAccount.AccountNumber;
                }      
                response.ResultCode = ResultCodes.normal;
                response.Description = Utils.GetActionResultErrors(response.Result.actionResult.Errors);

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ստուգում է ներբեռնվող Ecxel ֆայլը
        /// </summary>
        /// <param name="reestrTransferAdditionalDetails"></param>
        /// <param name="debetAccount"></param>
        /// <returns></returns>
        [HttpPost("CheckExcelRows")]
        public IActionResult CheckExcelRows(CheckExcelRowsRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xBService.CheckExcelRows(request.ReestrTransferAdditionalDetails, request.DebetAccount, request.OrderId);
                response.Result = result.Id;
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                response.Description = utils.GetActionResultErrors(result.Errors);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Պահպանում է ռեեստրի հայտը
        /// </summary>
        /// <param name="order"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpPost("SaveReestrTransferOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveReestrTransferOrder(SaveReestrTransferOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xBService.SaveReestrTransferOrder(request.Order, request.FileId);
                response.Result = result.Id;
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                response.Description = utils.GetActionResultErrors(result.Errors);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տարբեր արժույթով հաշիվների միջև փոխանցման հայտի տվյալները
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetCurrencyExchangeOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetCurrencyExchangeOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<PaymentOrder> response = new SingleResponse<PaymentOrder>();
                response.Result = _xBService.GetCurrencyExchangeOrder(request.Id);


                if (response.Result.Quality == OrderQuality.Declined)
                {
                    response.Result.RejectReason = _xBService.GetOrderRejectReason(request.Id, response.Result.Type);
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
        /// Միջազգային փոխանցման նախնական արժեքներ
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetInternationalOrderPrefilledData")]
        public IActionResult GetInternationalOrderPrefilledData()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<InternationalOrderPrefilledData> response = new SingleResponse<InternationalOrderPrefilledData>();
                response.Result = _xBService.GetInternationalOrderPrefilledData();
                response.ResultCode = ResultCodes.normal;
                
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ստանալ բանկի մասին ինֆորմացիա Swift կոդով
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetBankInfoViaSwiftCode")]
        public IActionResult GetBankInfoViaSwiftCode(SwiftCodeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                var details = _xBService.GetSearchedSwiftCodes(request.SwiftCode).FirstOrDefault();
                if (details != null)
                    response.Result = details.BankName + ", " + details.City;
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