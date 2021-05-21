using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;
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
    public class ReferenceController : ControllerBase
    {
        private readonly XBService _xbService;
        private readonly CacheHelper _cacheHelper;

        public ReferenceController(XBService xbService, CacheHelper cacheHelper)
        {
            _xbService = xbService;
            _cacheHelper = cacheHelper;
        }

        /// <summary>
        /// Պահպանում է Swift հաղորդագրության պատճենի ստացման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveSwiftCopyOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveSwiftCopyOrder(SwiftCopyOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveSwiftCopyOrder(request.Order);

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
        /// Ուղարկում է Swift հաղորդագրության պատճենի ստացման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveSwiftCopyOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.SwiftCopyOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveSwiftCopyOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                SwiftCopyOrder order = _cacheHelper.GetApprovalOrder<SwiftCopyOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveSwiftCopyOrder(order);

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
        /// Վերադարձնում է Swift պատճենի հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetSwiftCopyOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetSwiftCopyOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<SwiftCopyOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetSwiftCopyOrder(request.Id);
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
        /// Վերադարձնում է լիազորագրի հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCredentialOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetCredentialOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<CredentialOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCredentialOrder(request.Id);
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
        /// Պահպանում է լիազորագրի հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveCredentialOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveCredentialOrder(CredentialOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveCredentialOrder(request.Order);

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
        /// Լիազորագրի հայտի ուղարկում
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveCredentialOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.CredentialOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveCredentialOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                CredentialOrder order = _cacheHelper.GetApprovalOrder<CredentialOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveCredentialOrder(order);

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
        /// Վերադարձնում է տվյալների խմբագրման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCustomerDataOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetCustomerDataOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<CustomerDataOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCustomerDataOrder(request.Id);
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
        /// Պահպանում է տվյալների խմբագրման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveCustomerDataOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveCustomerDataOrder(CustomerDataOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveCustomerDataOrder(request.Order);

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
        /// Ուղարկում է տվյալների խմբագրման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveCustomerDataOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.CustomerDataOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveCustomerDataOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                CustomerDataOrder order = _cacheHelper.GetApprovalOrder<CustomerDataOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveCustomerDataOrder(order);

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
        /// Վերադարձնում է գործողության տեսակի հաշվեհամարները
        /// </summary>
        /// <param name="operationType"></param>
        /// <returns></returns>
        [HttpPost("GetAccountsForCredential")]
        public IActionResult GetAccountsForCredential(OperationTypeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Account>> response = new SingleResponse<List<Account>>();
                response.Result = _xbService.GetAccountsForCredential(request.OperationType);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Վերադարձնում է քաղվածքների էլեկտրոնային ստացման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetStatementByEmailOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetStatementByEmailOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<StatmentByEmailOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetStatementByEmailOrder(request.Id);
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
        /// Ուղարկում է քաղվածքների էլեկտրոնային ստացման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveStatementByEmailOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.StatmentByEmailOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveStatementByEmailOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                StatmentByEmailOrder order = _cacheHelper.GetApprovalOrder<StatmentByEmailOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveStatementByEmailOrder(order);

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
        /// Քաղվածքների էլեկտրոնային ստացման հայտի պահպանում
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveStatementByEmailOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveStatementByEmailOrder(StatmentByEmailOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveStatementByEmailOrder(request.Order);

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
        /// Վերադարձնում է տեղեկանքի ստացման հայտի տվյալները
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetReferenceOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetReferenceOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<ReferenceOrder>();
                response.Result = _xbService.GetReferenceOrder(request.Id);
                response.ResultCode = ResultCodes.normal;
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
        /// Պահպանում է տեղեկանքի ստացման հայտը
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost("SaveReferenceOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveReferenceOrder(ReferenceOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>();

                ActionResult result = _xbService.SaveReferenceOrder(request.Order);
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
        /// Հաստատում է տեղեկանքի ստացման հայտի տվյալները
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("ApproveReferenceOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.ReferenceOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveReferenceOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>();

                ReferenceOrder order = _cacheHelper.GetApprovalOrder<ReferenceOrder>(request.Id);
                ActionResult result = _xbService.ApproveReferenceOrder(order);
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
        /// Գումարի ստացման և փոխանցման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost("SaveCashOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveCashOrder(CashOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xbService.SaveCashOrder(request.Order);
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
        /// Գումարի ստացման և փոխանցման հայտի ուղարկում
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("ApproveCashOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.CashOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveCashOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<ActionResult>();
                CashOrder order = _cacheHelper.GetApprovalOrder<CashOrder>(request.Id);

                ActionResult Result = _xbService.ApproveCashOrder(order);

                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(Result.ResultCode);
                response.ResultCode = ResultCodes.normal;
                response.Description = utils.GetActionResultErrors(Result.Errors);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է գումարի ստացման և փոխանցման հայտը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetCashOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetCashOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<CashOrder> response = new SingleResponse<CashOrder>();
                response.Result = _xbService.GetCashOrder(request.Id);
                response.ResultCode = ResultCodes.normal;
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
        /// Վերադարձնում է հաճախորդի տվյալների խմբագրման հայտի համար անհրաժեշտ տվյալները
        /// </summary>
        /// <returns></returns>
        //[HttpPost("GetDataForCustomerDataOrder")]
        //public IActionResult GetDataForCustomerDataOrder()
        //{
        //    //SingleResponse<List<KeyValuePair<short, string>>> response = new SingleResponse<List<KeyValuePair<short, string>>>();
        //    //CustomerMainData mainData = _xbService.GetCustomerMainData();

        //    //List<CustomerEmail> emails = _xbService.GetEmails(request.CustomerNumber);

        //    //response.ResultCode = ResultCodes.normal;
        //    ////response.Result = _xBInfoService.GetPayIfNoDebtTypes();
        //    //return ResponseExtensions.ToHttpResponse(response);
        //}
    }
}