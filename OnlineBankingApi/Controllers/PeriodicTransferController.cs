using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingLibrary.Services;
using XBS;
using static OnlineBankingApi.Enumerations;
using ResponseExtensions = OnlineBankingApi.Models.ResponseExtensions;
using ActionResult = XBS.ActionResult;
using utils = OnlineBankingLibrary.Utilities.Utils;
using OnlineBankingApi.Utilities;
using OnlineBankingApi.Models.Requests;
using OnlineBankingLibrary;
using OnlineBankingLibrary.Utilities;
using Microsoft.Extensions.Localization;
using OnlineBankingApi.Resources;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class PeriodicTransferController : ControllerBase
    {
        private readonly XBService _xbService;
        private readonly CacheHelper _cacheHelper;
        private readonly IStringLocalizer _localizer;

        public PeriodicTransferController(XBService xbService, CacheHelper cacheHelper, IStringLocalizer<SharedResource> localizer)
        {
            _xbService = xbService;
            _cacheHelper = cacheHelper;
            _localizer = localizer;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի պարբերական փոխանցումների մանրամասները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPeriodicTransfers")]
        public IActionResult GetPeriodicTransfers(ProductQualityFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                var response = new SingleResponse<List<PeriodicTransfer>>() { ResultCode = ResultCodes.normal };
                var result = _xbService.GetPeriodicTransfers(request.Filter);
                result.RemoveAll(x => x.Type == 9);
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    response.Result.RemoveAll(m => !_xbService.HasProductPermissionByProductID(m.ProductId));
                }
                result.ForEach(m =>
                {
                    if (m.FeeAccount != null && (m.FeeAccount.AccountNumber == "0" || m.FeeAccount.AccountNumber == null || m.FeeAccount.AccountNumber == "-1"))
                        m.FeeAccount = null;
                    if (m.ChargeType == 0)
                        m.ChargeType = 1;
                    else
                        m.ChargeType = 2;
                });
                response.Result = result;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ պարբերական փոխանցման մանրամասները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPeriodicTransfer")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.PeriodicTransfer })]
        public IActionResult GetPeriodicTransfer(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<PeriodicTransfer>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetPeriodicTransfer(request.ProductId);
                if (response.Result != null)
                {
                    if (response.Result.FeeAccount != null && (response.Result.FeeAccount.AccountNumber == "0" || response.Result.FeeAccount.AccountNumber == null))
                        response.Result.FeeAccount = null;
                    if (response.Result.ChargeType == 0)
                        response.Result.ChargeType = 1;
                    else
                        response.Result.ChargeType = 2;
                }

                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        
        /// <summary>
        /// Վերադարձնում է տվյալ ժամանակահատվածում տվյալ պարբերական փոխանցման պատմությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPeriodicTransferHistory")]
        public IActionResult GetPeriodicTransferHistory(PeriodicTransferHistoryRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<PeriodicTransferHistory>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetPeriodicTransferHistory(request.ProductId, request.DateFrom, request.DateTo);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Ուղարկում է պարբերական փոխանցման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApprovePeriodicPaymentOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.PeriodicPaymentOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApprovePeriodicPaymentOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.ApprovePeriodicPaymentOrder(request.Id);

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
        /// Պահպանում է պարբերական փոխանցման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SavePeriodicPaymentOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SavePeriodicPaymentOrder(PeriodicPaymentOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };
                if (request.Order.ChargeType == 1)
                    request.Order.ChargeType = 0;
                else if (request.Order.ChargeType == 2)
                    request.Order.ChargeType = 1;

                if (request.Order.SubType == 3)
                    request.Order.PeriodicDescription = "Փոխանցում հաշիվին";
                ActionResult saveResult = _xbService.SavePeriodicPaymentOrder(request.Order);
                    
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(saveResult.ResultCode);
                response.Result = saveResult.Id;
                response.Description = utils.GetActionResultErrors(saveResult.Errors);

                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                if (request == null)
                {
                    foreach (var item in ModelState.Keys)
                    {
                        if(item.ToString().ToLower() == "order.CheckDaysCount".ToLower())
                        {
                            return ValidationError.GetTypeValidationErrorResponse(item, _localizer["Սխալ ստուգման օրերի քանակ"]);
                        }
                    }
                }
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Պահպանում է պարբերական փոխանցման (բյուջե) հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveBudgetPeriodicPaymentOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveBudgetPeriodicPaymentOrder(PeriodicBudgetPaymentOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveBudgetPeriodicPaymentOrder(request.Order);

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
        /// Պահպանում է պարբերական փոխանցման (կոմունալ) հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveUtilityPeriodicPaymentOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveUtilityPeriodicPaymentOrder(PeriodicUtilityPaymentOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };
                ActionResult saveResult = _xbService.SaveUtilityPeriodicPaymentOrder(request.Order);
                
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(saveResult.ResultCode);
                response.Result = saveResult.Id;
                response.Description = utils.GetActionResultErrors(saveResult.Errors);

                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                if (request == null)
                {
                    foreach (var item in ModelState.Keys)
                    {
                        if (item.ToString().ToLower() == "order.CheckDaysCount".ToLower())
                        {
                            return ValidationError.GetTypeValidationErrorResponse(item, _localizer["Սխալ ստուգման օրերի քանակ"]);
                        }
                    }
                }
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Պարբերական փոխանցման հայտի տվյալների ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPeriodicPaymentOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetPeriodicPaymentOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xbService.GetPeriodicPaymentOrder(request.Id);
                var response = new SingleResponse<PeriodicPaymentOrder>();
                if (authorizedCustomer.LimitedAccess!= 0)
                {
                    if (_xbService.HasProductPermission(order.DebitAccount.AccountNumber))
                    {
                        hasPermission = false;
                        response.ResultCode = ResultCodes.failed;
                        response.Result.Description = "Տվյալները հասանելի չեն։";
                    }
                }
                if (hasPermission)
                {
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
        /// Պարբերական փոխանցման հայտի (բյուջե) տվյալների ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPeriodicBudgetPaymentOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetPeriodicBudgetPaymentOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xbService.GetPeriodicBudgetPaymentOrder(request.Id);
                var response = new SingleResponse<PeriodicBudgetPaymentOrder>();
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    if (!_xbService.HasProductPermission(order.DebitAccount.AccountNumber))
                    {
                        hasPermission = false;
                        response.ResultCode = ResultCodes.failed;
                        response.Result.Description = "Տվյալները հասանելի չեն։";
                    }
                }

                if (hasPermission)
                {
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
        /// Պարբերական փոխանցման հայտի (կոմունալ) տվյալների ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPeriodicUtilityPaymentOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetPeriodicUtilityPaymentOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xbService.GetPeriodicUtilityPaymentOrder(request.Id);
                var response = new SingleResponse<PeriodicUtilityPaymentOrder>();
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    if (!_xbService.HasProductPermission(order.DebitAccount.AccountNumber))
                    {
                        hasPermission = false;
                        response.ResultCode = ResultCodes.failed;
                        response.Result.Description = "Տվյալները հասանելի չեն։";
                    }
                }

                if (hasPermission)
                {
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
        /// Պարբերականի փակման հայտի պահպանում
        /// </summary>
        /// <returns></returns>
        [HttpPost("SavePeriodicTerminationOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SavePeriodicTerminationOrder(PeriodicTerminationOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                request.Order.OrderNumber = "";
                request.Order.RegistrationDate = _xbService.GetNextOperDay();
                ActionResult saveResult = _xbService.SavePeriodicTerminationOrder(request.Order);

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
        /// Պարբերականի փակման հայտի հաստատում
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApprovePeriodicTerminationOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.PeriodicTerminationOrder })]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApprovePeriodicTerminationOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                PeriodicTerminationOrder order = _xbService.GetPeriodicTerminationOrder(request.Id);

                ActionResult saveResult = _xbService.ApprovePeriodicTerminationOrder(order);

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
        ///Պարբերականի փակման հայտի հաստատում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPeriodicTerminationOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetPeriodicTerminationOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<PeriodicTerminationOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetPeriodicTerminationOrder(request.Id);
                if (response.Result.Quality == OrderQuality.Declined)
                {
                    response.Result.RejectReason = _xbService.GetOrderRejectReason(request.Id, response.Result.Type);
                }
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Պարբերական փոխանցման փոփոխման հայտի պահպանում
        /// </summary>
        /// <returns></returns>
        [HttpPost("SavePeriodicDataChangeOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SavePeriodicDataChangeOrder(PeriodicTransferDataChangeOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };
                if (request.Order.ChargeType == 1)
                    request.Order.ChargeType = 0;
                else if (request.Order.ChargeType == 2)
                    request.Order.ChargeType = 1;
                ActionResult saveResult = _xbService.SavePeriodicDataChangeOrder(request.Order);

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
        /// Պարբերական փոխանցման փոփոխման հայտի հաստատում
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApprovePeriodicDataChangeOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.PeriodicDataChangeOrder })]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApprovePeriodicDataChangeOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                PeriodicTransferDataChangeOrder order = _xbService.GetPeriodicDataChangeOrder(request.Id);

                ActionResult saveResult = _xbService.ApprovePeriodicDataChangeOrder(order);

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
        /// Պարբերական փոխանցման փոփոխման հայտի տվյալների ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPeriodicDataChangeOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetPeriodicDataChangeOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xbService.GetPeriodicDataChangeOrder(request.Id);
                var response = new SingleResponse<PeriodicTransferDataChangeOrder>();
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    if (_xbService.HasProductPermission(order.DebitAccount.AccountNumber))
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
                        order.RejectReason = _xbService.GetOrderRejectReason(request.Id, order.Type);
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
        /// Վերադարձնում է Պարբերականների քանակը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetPeriodicTransfersCount")]
        public IActionResult GetPeriodicTransfersCount(PeriodicTransfersTypeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<int> response = new SingleResponse<int>();
                response.Result = _xbService.GetPeriodicTransfersCount(request.PeriodicType);
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