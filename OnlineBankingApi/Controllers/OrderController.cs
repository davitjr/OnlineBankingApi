using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using ArcaCardAttachmentService;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json.Linq;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
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
    public class OrderController : ControllerBase
    {
        private readonly XBService _xBService;
        private readonly XBInfoService _xBInfoService;
        private readonly CacheHelper _cacheHelper;
        private readonly ArcaCard.ArcaCardClient _client;
        private readonly IConfiguration _config;
        public OrderController(XBService xBService, XBInfoService xBInfoService, CacheHelper cacheHelper, ArcaCard.ArcaCardClient client, IConfiguration config)
        {
            _xBService = xBService;
            _xBInfoService = xBInfoService;
            _cacheHelper = cacheHelper;
            _client = client;
            _config = config;
        }

        /// <summary>
        /// Վերադարձնում է քարտի միջնորդավճարը
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost("GetCardFee")]
        public IActionResult GetCardFee(CardFeeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBService.GetCardFee(request.Order);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է Swift հաղորդագրության պատճենի ստացման հայտի միջնորդավճարը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetSwiftCopyOrderFee")]
        public IActionResult GetSwiftCopyOrderFee()
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<double>() { ResultCode = ResultCodes.normal };
                response.Result = _xBService.GetSwiftCopyOrderFee();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Պահպանում և ուղարկում է գործարքի հեռացման հայտը
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost("SaveAndApproveRemovalOrder")]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.RemovalOrder })]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult SaveAndApproveRemovalOrder(RemovalOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                request.Order.Quality = OrderQuality.Draft;
                request.Order.RemovingReason = 9;
                request.Order.RemovingReasonAdd = "Գործարքից հրաժարում"; //language պետք է ավելացնել
                ActionResult result = _xBService.SaveAndApproveRemovalOrder(request.Order);
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
        /// Վերադարձնում է գործարքի հեռացման հայտի տվյալները
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetRemovalOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetRemovalOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<RemovalOrder> response = new SingleResponse<RemovalOrder>();
                response.Result = _xBService.GetRemovalOrder(request.Id);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաշվի մնացորդը վճարման հանձնարարականի կատարումից հետո
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetPaymentOrderFutureBalance")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetPaymentOrderFutureBalance(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<PaymentOrderFutureBalance> response = new SingleResponse<PaymentOrderFutureBalance>();
                response.Result = _xBService.GetPaymentOrderFutureBalance(request.Id);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է վճարման հանձնարարականի միջնորդավճարի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost("GetPaymentOrderFee")]
        public IActionResult GetPaymentOrderFee(PaymentOrderFeeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBService.GetPaymentOrderFee(request.Order);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ տեսակի հայտի համար հասանելի հաշիվների ցանկը` ըստ տեսակի (դեբետագրվող/կրեդիտագրվող)
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAccountsForOrder")]
        public IActionResult GetAccountsForOrder(AccountForOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                SingleResponse<List<Account>> response = new SingleResponse<List<Account>>();
                response.ResultCode = ResultCodes.normal;
                List<Account> accounts = _xBService.GetAccountsForOrder(request.OrderType, request.OrderSubType, request.AccountType);

                //Դեբետ հաշվի դեպքում ջնջում ենք ապառիկի հաշիվները
                if (request.AccountType == 1)
                {
                    accounts.RemoveAll(m => m.AccountType == 54 || m.AccountType == 58);
                }
                else if (request.AccountType == 2)
                {
                    if (accounts.Exists(m => m.AccountType == 54 || m.AccountType == 58))
                    {
                        List<Loan> aparikTexumLoans = _xBService.GetAparikTexumLoans();
                        foreach (Account account in accounts.FindAll(m => m.AccountType == 54 || m.AccountType == 58))
                        {
                            if (!aparikTexumLoans.Exists(m => m.ConnectAccount.AccountNumber == account.AccountNumber))
                            {
                                accounts.RemoveAll(m => m.AccountNumber == account.AccountNumber);
                            }
                        }

                    }
                }

              
                    //await accounts.ToList().ForEachAsync(async m => {
                    //    if (m.AccountType == 11)
                    //    {
                    //        m.ArcaBalance = _xBService.GetArcaBalance(m.AccountDescription.Substring(0, 16).Trim());
                    //        if (m.AccountNumber == "220000295380000" && Convert.ToBoolean(_config["TestVersion"]))
                    //            m.ArcaBalance = null;
                    //    }
                    //});
                

                Parallel.ForEach(accounts, m =>
                {
                    if (m.AccountType == 11)
                    {
                        m.ArcaBalance = _xBService.GetArcaBalance(m.AccountDescription.Substring(0, 16).Trim());
                        if (m.AccountNumber == "220000295380000" && Convert.ToBoolean(_config["TestVersion"]))
                            m.ArcaBalance = null;
                    }
                });
                response.Result = accounts;
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    response.Result.RemoveAll(m => !_xBService.HasProductPermission(m.AccountNumber));
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        //public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
        //{
        //    foreach (var value in list)
        //    {
        //        await func(value);
        //    }
        //}

        /// <summary>
        /// Վերադարձնում է տեղեկանքի ստացման հայտի միջնորդավճարը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetReferenceOrderFee")]
        public IActionResult GetReferenceOrderFee(UrgentSignRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xBService.GetReferenceOrderFee(request.UrgentSign);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }



        /// <summary>
        /// Վերադարձնում է հաճախորդի՝ "Խմբագրվող" կարգավիճակով գործարքները
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        [HttpPost("GetDraftOrders")]
        public IActionResult GetDraftOrders(DateFromDateToRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Order>> response = new SingleResponse<List<Order>>();
                response.Result = _xBService.GetDraftOrders(request.DateFrom, request.DateTo);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի ուղարկված գործարքները
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        [HttpPost("GetSentOrders")]
        public IActionResult GetSentOrders(DateFromDateToRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Order>> response = new SingleResponse<List<Order>>();
                response.Result = _xBService.GetSentOrders(request.DateFrom, request.DateTo);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի՝ "Հաստատված" կարգավիճակով գործարքները
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        [HttpPost("GetApproveReqOrder")]
        public IActionResult GetApproveReqOrder(DateFromDateToRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Order>> response = new SingleResponse<List<Order>>();
                response.Result = _xBService.GetApproveReqOrder(request.DateFrom, request.DateTo);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է որոնման տվյալներին համապատասխանող հանձնարարականները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetOrders")]
        public IActionResult GetOrders(OrderFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Order>> response = new SingleResponse<List<Order>>();
                response.Result = _xBService.GetOrders(request.OrderFilter);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Վերադարձնում է տվյալ հայտի պատմությունը
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost("GetOnlineOrderHistory")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetOnlineOrderHistory(OrderIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<OrderHistory>> response = new SingleResponse<List<OrderHistory>>();
                response.Result = _xBService.GetOnlineOrderHistory((int)request.OrderId);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաստատման ենթակա վճարման հանձնարարականները
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpPost("GetConfirmRequiredOrders")]
        public IActionResult GetConfirmRequiredOrders(StartDateEndDateRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<dynamic>> response = new SingleResponse<List<dynamic>>();
                response.Result = new List<dynamic>();
                AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                List<Order> order = _xBService.GetConfirmRequiredOrders(request.StartDate, request.EndDate, authorizedCustomer.UserName);
                if (_cacheHelper.GetSourceType() == SourceType.MobileBanking)
                {
                    order.ForEach(x =>
                    {
                        switch (x.Type)
                        {
                            case OrderType.RATransfer:
                            case OrderType.Convertation:
                                {
                                    PaymentOrder order = _xBService.GetPaymentOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { ReceiverAccountNumber = order.ReceiverAccount.AccountNumber });
                                    response.Result.Add(result);
                                }
                                break;
                            case OrderType.PlasticCardOrder:
                            case OrderType.AttachedPlasticCardOrder:
                            case OrderType.LinkedPlasticCardOrder:
                                {
                                    PlasticCardOrder order = _xBService.GetPlasticCardOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { PlasticCardCurrency = order.PlasticCard.Currency });
                                    response.Result.Add(result);
                                }
                                break;
                            case OrderType.CommunalPayment:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.LoanMature:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.ReferenceOrder:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.CreditSecureDeposit:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.ReceivedFastTransferPaymentOrder:
                                {
                                    ReceivedFastTransferPaymentOrder order = _xBService.GetReceivedFastTransferPaymentOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { ReceiverAccountNumber = order.ReceiverAccount.AccountNumber });
                                    response.Result.Add(result);
                                }
                                break;
                            case OrderType.CurrentAccountClose:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.SwiftCopyOrder:
                                {
                                    SwiftCopyOrder order = _xBService.GetSwiftCopyOrder(x.Id);
                                    if (order.FeeAccount != null)
                                    {
                                        var result = TypeMerger.TypeMerger.Merge(x, new { FeeAccountNumber = order.FeeAccount.AccountNumber });
                                        response.Result.Add(result);
                                    }
                                    else
                                        response.Result.Add(x);
                                }
                                break;
                            case OrderType.CredentialOrder:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.Deposit:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.CurrentAccountOpen:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.CashOrder:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.CreditLineMature:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.CardClosing:
                                {
                                    CardClosingOrder order = _xBService.GetCardClosingOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { order.ProductId });
                                    response.Result.Add(result);
                                }
                                break;
                            case OrderType.CustomerDataOrder:
                                {
                                    CustomerDataOrder order = _xBService.GetCustomerDataOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { order.Password });
                                    response.Result.Add(result);
                                }
                                break;
                            case OrderType.StatmentByEmailOrder:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.DepositTermination:
                                {
                                    DepositTerminationOrder order = _xBService.GetDepositTerminationOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { order.ProductId });
                                    response.Result.Add(result);
                                }
                                break;
                            case OrderType.ReestrTransferOrder:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.ArcaCardsTransactionOrder:
                                {
                                    ArcaCardsTransactionOrder order = _xBService.GetArcaCardsTransactionOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { order.CardNumber });
                                    response.Result.Add(result);
                                }
                                break;
                            case OrderType.CardToCardOrder:
                                {
                                    CardToCardOrder order = _xBService.GetCardToCardOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { order.DebitCardNumber });
                                    var SecondResult = TypeMerger.TypeMerger.Merge(result, new { order.CreditCardNumber });
                                    response.Result.Add(SecondResult);
                                }
                                break;
                            case OrderType.CardLimitChangeOrder:
                                {
                                    CardLimitChangeOrder order = _xBService.GetCardLimitChangeOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { order.Limits[0].LimitValue });
                                    response.Result.Add(result);
                                }
                                break;
                            case OrderType.PeriodicTransfer:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.InternationalTransfer:
                                {
                                    InternationalPaymentOrder order = _xBService.GetInternationalPaymentOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { ReceiverAccountNumber = order.ReceiverAccount.AccountNumber });
                                    response.Result.Add(result);
                                }
                                break;
                            case OrderType.CurrentAccountReOpen:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.PlasticCardSMSServiceOrder:
                                {
                                    response.Result.Add(x);
                                }
                                break;
                            case OrderType.CancelTransaction:
                            case OrderType.RemoveTransaction:
                                {
                                    RemovalOrder order = _xBService.GetRemovalOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { order.RemovingOrderId });
                                    response.Result.Add(result);
                                }
                                break;
                            case OrderType.PeriodicTransferStop:
                                {
                                    PeriodicTerminationOrder order = _xBService.GetPeriodicTerminationOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { order.ProductId });
                                    response.Result.Add(result);
                                }
                                break;
                            case OrderType.PeriodicTransferDataChangeOrder:
                                {
                                    PeriodicTransferDataChangeOrder order = _xBService.GetPeriodicDataChangeOrder(x.Id);
                                    var result = TypeMerger.TypeMerger.Merge(x, new { order.ProductId });
                                    response.Result.Add(result);
                                }
                                break;
                        }
                    });
                }
                else
                {
                    response.Result.AddRange(order);
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
        /// Վերադարձնում է ռեեստրով վճարման հանձնարարականի տվյալները
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetReestrTransferOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetReestrTransferOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xBService.GetReestrTransferOrder(request.Id);
                SingleResponse<ReestrTransferOrder> response = new SingleResponse<ReestrTransferOrder>();
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    if (!_xBService.HasProductPermission(order.DebitAccount.AccountNumber))
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
        /// Հաստատում/ուղարկում է ռեեստրով վճարման հայտը)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("ApproveReestrTransferOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.ReestrTransferOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveReestrTransferOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                ReestrTransferOrder order = _cacheHelper.GetApprovalOrder<ReestrTransferOrder>(request.Id);
                XBS.ActionResult result = _xBService.ApproveReestrTransferOrder(order);
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
        /// Վերադարձնում է քարտից քարտ փոխանցման միջնորդավճարը
        /// </summary>
        /// <param name="debitCardNumber"></param>
        /// <param name="creditCardNumber"></param>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        [HttpPost("GetCardToCardTransferFee")]
        public IActionResult GetCardToCardTransferFee(CardToCardTransferFeeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.Result = _xBService.GetCardToCardTransferFee(request.DebitCardNumber, request.CreditCardNumber, request.Amount, request.Currency);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քարտից քարտ փոխանցման հայտը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetCardToCardOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetCardToCardOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<CardToCardOrder> response = new SingleResponse<CardToCardOrder>();
                response.Result = _xBService.GetCardToCardOrder(request.Id);
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
        /// Պահպանում է քարտից քարտ փոխանցման հայտը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SaveCardToCardOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public async Task<IActionResult> SaveCardToCardOrder(CardToCardOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                ActionResult result;
                if (!request.Order.IsAttachedCard)
                {
                    result = _xBService.SaveCardToCardOrder(request.Order);
                    response.Result = result.Id;
                    response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                    response.Description = utils.GetActionResultErrors(result.Errors);
                    return ResponseExtensions.ToHttpResponse(response);
                }
                else
                {
                    var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                    var language = _cacheHelper.GetLanguage();
                    var source = _cacheHelper.GetSourceType();
                    var bindpayresponse = await _client.ArcaOrderBindingPaymentAsync(new CardBindingPaymentRequest
                    {
                        BindingId = request.Order.BindingId,
                        Amount = (int)request.Order.Amount,
                        Currency = request.Order.Currency,
                        OrderType = CardBindingPaymentRequest.Types.OrderTypeEnum.CardToCardOrder,
                        OrderSubType = request.Order.SubType,
                        CustomerNumber = authorizedCustomer.CustomerNumber,
                        UserId = authorizedCustomer.UserId,
                        Language = language == 1 ? "hy" : "en",
                        PageView = source == SourceType.AcbaOnline ? "DESKTOP" : "MOBILE"
                    });
                    if (bindpayresponse.Payed)
                    {
                        //Save Details To New Table !!!!!!!!!!!!!!!!!
                        //var approveResult = _xBService.ToCardWithECommerce(request.Order);
                        //if (approveResult.ResultCode == ResultCode.Normal)
                        //{
                        return ResponseExtensions.ToHttpResponse(new Response
                        {
                            ResultCode = ResultCodes.normal,
                            Description = "Փոխանցումը կատարված է:"
                        });
                        //}
                        //else
                        //{
                        //    _ = await _client.ReverseOrderByIdAsync(new ReverseOrderRequest
                        //    {
                        //        MdOrder = bindpayresponse.MdOrder,
                        //        UserId = authorizedCustomer.UserId
                        //    });
                        //    return ResponseExtensions.ToHttpResponse(new Response
                        //    {
                        //        ResultCode = ResultCodes.failed,
                        //        Description = "Փոխանցման ժամանակ տեղի ունեցավ սխալ, խնդրում ենք դիմել բանկ:"
                        //    });
                        //}
                    }
                    else
                    {
                        return ResponseExtensions.ToHttpResponse(new Response
                        {
                            ResultCode = ResultCodes.failed,
                            Description = "Փոխանցման ժամանակ տեղի ունեցավ սխալ, խնդրում ենք դիմել բանկ::"
                        });
                    }
                }
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Քարտից քարտ փոխանցման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("ApproveCardToCardOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.CardToCardOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveCardToCardOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                CardToCardOrder order = _cacheHelper.GetApprovalOrder<CardToCardOrder>(request.Id);
                XBS.ActionResult result = _xBService.ApproveCardToCardOrder(order);
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
        /// ՀՀ տարածքում/հաշիվների միջև փոխանցման ձևանմուշի/խմբային ծառայության պահպանում/խմբագրում
        /// </summary>
        /// <param name="tesmplate"></param>
        /// <returns></returns>
        [HttpPost("SavePaymentOrderTemplate")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SavePaymentOrderTemplate(PaymentOrderTemplateRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xBService.SavePaymentOrderTemplate(request.Template);
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
        /// Տոկենի Ծառայությունների ակտիվացման հայտի Get
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetHBApplicationOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetHBApplicationOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<HBApplicationOrder> response = new SingleResponse<HBApplicationOrder>();
                response.Result = _xBService.GetHBApplicationOrder(request.Id);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Տոկենի ակտիվացման հայտի Get
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetHBActivationOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetHBActivationOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<HBActivationOrder> response = new SingleResponse<HBActivationOrder>();
                response.Result = _xBService.GetHBActivationOrder(request.Id);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Գործարքների խմբի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost("SaveOrderGroup")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveOrderGroup(SaveOrderGroupRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();

                request.Group.UserName = authorizedCustomer.UserName;
                XBS.ActionResult result = _xBService.SaveOrderGroup(request.Group, authorizedCustomer.UserId);
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
        /// Միաժամանակ 1-ից ավելի գործարքների հաստատում
        /// </summary>
        /// <param name="ListDocId"></param>
        /// <returns></returns>
        [HttpPost("ApproveOrders")]       
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.MultipleOrders })]
        public IActionResult ApproveOrders(ListDocIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<SingleResponse<long>>> response = new SingleResponse<List<SingleResponse<long>>>();
                response.Result = new List<SingleResponse<long>>();
                response.ResultCode = ResultCodes.failed;
                foreach (int docId in request.ListDocId)
                {
                    SingleResponse<long> res = new SingleResponse<long>();
                    res.ResultCode = (ResultCodes)ResultCode.Failed;
                    res.Result = docId;
                    res.Description = "Error!Please try later.";
                    response.Result.Add(res);
                }

                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();

                try
                {
                    foreach (SingleResponse<long> singleResponse in response.Result)
                    {
                        Order order = new Order();
                        order.Id = singleResponse.Result;
                        order.Type = _xBService.GetDocumentType(Convert.ToInt32(singleResponse.Result));

                        if (authorizedCustomer.Permission != 3 && authorizedCustomer.Permission != 2)
                        {
                            byte language = _cacheHelper.GetLanguage() == 0 ? (byte)2 : (byte)1;

                            singleResponse.ResultCode = ResultCodes.validationError;
                            singleResponse.Description = _xBService.GetTerm(1745, null, (Languages)language);
                            response.Description = singleResponse.Description;
                            break;
                        }


                        if (order.Type == XBS.OrderType.NotDefined)
                        {
                            response.Description += singleResponse.Result + " - " + singleResponse.Description;
                            continue;
                        }
                        else
                        {
                            try
                            {
                                string actionErrors = "";

                                if ((!_xBService.IsAbleToChangeQuality(authorizedCustomer.UserName, (int)order.Id) && order.Type != OrderType.RemoveTransaction)
                                     || (order.Type == OrderType.RemoveTransaction && !authorizedCustomer.IsLastConfirmer))
                                {

                                    byte language = _cacheHelper.GetLanguage() == 0 ? (byte)2 : (byte)1;

                                    singleResponse.ResultCode = ResultCodes.validationError;
                                    singleResponse.Description = _xBService.GetTerm(1689, null, (Languages)language);
                                    response.Description += singleResponse.Result + " - " + singleResponse.Description;
                                  
                                }
                                else
                                {
                                    ActionResult saveResult = _xBService.ApproveOrder(order);
                                    singleResponse.ResultCode = (ResultCodes)saveResult.ResultCode;

                                    if (saveResult.ResultCode == ResultCode.Normal || saveResult.ResultCode == ResultCode.SaveAndSendToConfirm)
                                    {
                                        response.ResultCode = ResultCodes.normal;
                                    }

                                    if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                                    {
                                        saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                                        actionErrors = actionErrors.Remove(actionErrors.Length - 1);
                                        response.Description += singleResponse.Result + " - " + actionErrors;
                                        singleResponse.Description = actionErrors;
                                    }
                                    else
                                    {
                                        if (saveResult.ResultCode != ResultCode.Failed)
                                        {
                                            singleResponse.Description = actionErrors;
                                        }
                                    }
                                }


                               
                            }
                            catch (Exception ex)
                            {
                                singleResponse.ResultCode = ResultCodes.failed;
                                response.Description += singleResponse.Result + " - " + singleResponse.Description;
                                singleResponse.Description = ex.Message;
                                continue;
                            }
                        }
                    }
                }
                catch (FaultException ex)
                {
                    //պետք է փոխվի
                }
                catch (Exception ex)
                {
                    //պետք է փոխվի
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }

        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի՝ գործարքները(կախված հայտի տեսակից, գործարքի կարգավիճակից
        /// </summary>
        /// <param name="orderListFilter"></param>
        /// <returns></returns>
        [HttpPost("GetOrdersList")]
        public IActionResult GetOrdersList(OrderListFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Order>> response = new SingleResponse<List<Order>>();
                response.Result = _xBService.GetOrdersList(request.OrderListFilter);
                foreach (var item in response.Result)
                {
                    if (item.Type == OrderType.LoanMature)
                    {
                        if (item.SubType == 2) item.SubType = 9;
                        else if (item.SubType == 6) item.SubType = 2;
                        item.SubTypeDescription = _xBInfoService.GetLoanMatureTypesForIBanking()
                            .Where(x => x.Key == item.SubType.ToString())
                            .FirstOrDefault().Value;
                        if (item.SubType == 4) item.SubType = 2;
                        item.Description = item.SubTypeDescription;
                    }
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
        /// Պահպանում է փոխարկման հայտը
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost("SaveCurrencyExchangeOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveCurrencyExchangeOrder(CurrencyExchangeOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xBService.SaveCurrencyExchangeOrder(request.Order);
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
        /// Փոխարկման դեպքում գումարից և արժույթից կախված արժույթի դաշտը փոփոխելու հնարավորության թույլատրում
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("ManuallyRateChangingAccess")]
        public IActionResult ManuallyRateChangingAccess(ManuallyRateChangingAccessRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<bool> response = new SingleResponse<bool>();
                response.Result = _xBService.ManuallyRateChangingAccess(request.Amount, request.Currency);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        [HttpPost("RejectOrder")]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult RejectOrder(OrderRejectionRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xBService.RejectOrder(request.OrderRejection);
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
        /// Վերադարձնում է հայտին կցված փաստաթուղթը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetOrderAttachmentInBase64")]
        public IActionResult GetOrderAttachmentInBase64(GUIDRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.Result = _xBService.GetOrderAttachmentInBase64(request.Id);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Միաժամանակ 1-ից ավելի գործարքների հաստատում
        /// </summary>
        /// <param name="ListDocId"></param>
        /// <returns></returns>
        [HttpPost("ApproveOrdersAsync")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.MultipleOrders })]
        public IActionResult ApproveOrdersAsync(ListDocIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<SingleResponse<long>>> response = new SingleResponse<List<SingleResponse<long>>>();
                String description = "";
                response.Result = new List<SingleResponse<long>>();
                response.ResultCode = ResultCodes.failed;
                foreach (int docId in request.ListDocId)
                {
                    SingleResponse<long> res = new SingleResponse<long>();
                    res.ResultCode = (ResultCodes)ResultCode.Failed;
                    res.Result = docId;
                    res.Description = "Error!Please try later.";
                    response.Result.Add(res);
                }

                Parallel.ForEach(response.Result, singleResponse =>
                {
                    Order order = new Order();
                    order.Id = singleResponse.Result;
                    order.Type = _xBService.GetDocumentType(Convert.ToInt32(singleResponse.Result));

                    if (order.Type == XBS.OrderType.NotDefined)
                    {
                        description += singleResponse.Result + " - " + singleResponse.Description + Environment.NewLine;
                    }
                    else
                    {
                        try
                        {
                            ActionResult saveResult = _xBService.ApproveOrder(order);
                            singleResponse.ResultCode = (ResultCodes)saveResult.ResultCode;

                            if (saveResult.ResultCode == ResultCode.Normal || saveResult.ResultCode == ResultCode.SaveAndSendToConfirm)
                            {
                                response.ResultCode = ResultCodes.normal;
                            }
                            string actionErrors = "";
                            if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                            {
                                saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                                actionErrors = actionErrors.Remove(actionErrors.Length - 1);

                                description += singleResponse.Result + " - " + actionErrors + Environment.NewLine;
                                singleResponse.Description = actionErrors;
                            }
                            else
                            {
                                if (saveResult.ResultCode != ResultCode.Failed)
                                {
                                    singleResponse.Description = actionErrors;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            singleResponse.ResultCode = ResultCodes.failed;
                            description += singleResponse.Result + " - " + singleResponse.Description + Environment.NewLine;
                            singleResponse.Description = ex.Message;
                        }
                    }
                });


                response.Description = description;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }

        }

        /// <summary>
        /// միջնորդավճարի ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetOrderServiceFee")]
        public IActionResult GetOrderServiceFee(ServiceFeeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.Result = _xBService.GetOrderServiceFee(request.Type, request.Urgent);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հայտին կցված փաստաթղթերը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetOrderAttachments")]
        public IActionResult GetOrderAttachments(OrderIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<OrderAttachment>> response = new SingleResponse<List<OrderAttachment>>();
                response.Result = _xBService.GetOrderAttachments(request.OrderId);
                response.ResultCode = ResultCodes.normal;

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հայտին կցված փաստաթուղթը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetOrderAttachment")]
        public IActionResult GetOrderAttachment(GUIDRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<OrderAttachment> response = new SingleResponse<OrderAttachment>();
                response.Result = _xBService.GetOrderAttachment(request.Id);
                response.ResultCode = ResultCodes.normal;

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաստատման ենթակա վճարման հանձնարարականների քանակը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetConfirmRequiredOrdersCount")]
        public IActionResult GetConfirmRequiredOrdersCount()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<int> response = new SingleResponse<int>();
                AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                response.Result = _xBService.GetConfirmRequiredOrders(default, default, authorizedCustomer.UserName).Count;
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