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
using System.Threading.Tasks;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class DepositController : ControllerBase
    {
        private readonly XBService _xbService;
        private readonly ContractManager _contractManager;
        private readonly CacheHelper _cacheHelper;

        public DepositController(XBService xbService, ContractManager contractManager, CacheHelper cacheHelper)
        {
            _xbService = xbService;
            _contractManager = contractManager;
            _cacheHelper = cacheHelper;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի ավանդների մասին տեղեկատվությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDeposits")]
        public IActionResult GetDeposits(ProductQualityFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                SingleResponse<List<Deposit>> response = new SingleResponse<List<Deposit>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetDeposits(request.Filter);
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    response.Result.RemoveAll(m => _xbService.HasProductPermission(m.DepositAccount.AccountNumber, (ulong)m.ProductId));
                }

                Parallel.ForEach(response.Result, dep =>
                {
                    dep.ProductNote = _xbService.GetProductNote(Convert.ToDouble(dep.ProductId));
                });

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ ավանդի մասին տեղեկատվությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDeposit")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Deposit })]
        public IActionResult GetDeposit(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<Deposit> response = new SingleResponse<Deposit>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetDeposit(request.ProductId);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ավանդի վճարման գրաֆիկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDepositRepaymentGrafik")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Deposit })]
        public IActionResult GetDepositRepaymentGrafik(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<DepositRepayment>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetDepositRepaymentGrafik(request.ProductId);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ավանդի բացման հայտի ուղարկում
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveDepositOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.DepositOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveDepositOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                DepositOrder order = _cacheHelper.GetApprovalOrder<DepositOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveDepositOrder(order);

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
        /// Ավանդի բացման հայտի պահպանում
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveDepositOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveDepositOrder(DepositOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveDepositOrder(request.Order);

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
        /// Վերադարձնում է տվյալ ավանդատեսակին համապատասխան պայմանները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDepositCondition")]
        public IActionResult GetDepositCondition(DepositConditionRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<DepositOrderCondition>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetDepositCondition(request.Order);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ ավանդին համապատասխան տոկոսագումարի հաշիվների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAccountsForDepositPercentAccount")]
        public IActionResult GetAccountsForDepositPercentAccount(AccountsForDepositPercentAccountRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<Account>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetAccountsForNewDeposit(request.Order);

                if (request.Order.Currency != "AMD")
                {
                    request.Order.Currency = "AMD";
                    List<Account> AMDAccounts = _xbService.GetAccountsForNewDeposit(request.Order);
                    response.Result.AddRange(AMDAccounts);
                }

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Վերադարձնում է տվյալ ավանդին համապատասխան մայր գումարի հաշիվների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAccountsForNewDeposit")]
        public IActionResult GetAccountsForNewDeposit(AccountsForNewDepositRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<Account>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetAccountsForNewDeposit(request.Order);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրաֆիկը՝ նախքան ավանդի ձևակերպումը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDepositRepaymentsPrior")]
        public IActionResult GetDepositRepaymentsPrior(DepositRepaymentsPriorRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<DepositRepayment>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetDepositRepaymentsPrior(request.Request);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Վերադարձնում է ավանդի դադարեցման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDepositTerminationOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetDepositTerminationOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xbService.GetDepositTerminationOrder(request.Id);
                order.Deposit = _xbService.GetDeposit(order.ProductId);
                var response = new SingleResponse<DepositTerminationOrder>();
                //response.Result = _xbService.GetDepositTerminationOrder(request.Id); - ??
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    if (!_xbService.HasProductPermission(order.DebitAccount.AccountNumber))
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
        /// Ուղարկում է ավանդի դադարեցման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveDepositTerminationOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.DepositTerminationOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveDepositTerminationOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                DepositTerminationOrder order = _cacheHelper.GetApprovalOrder<DepositTerminationOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveDepositTerminationOrder(order);

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
        /// Պահպանում է ավանդի դադարեցման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveDepositTerminationOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveDepositTerminationOrder(DepositTerminationOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveDepositTerminationOrder(request.Order);

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
        /// Վերադարձնում է ավանդի բացման հայտի տվյալները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDepositOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetDepositOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<DepositOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetDepositOrder(request.Id);
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
        /// Վերադարձնում է տվյալ պայմաններով ավանդի ստուգումները
        /// </summary>
        /// <returns></returns>
        [HttpPost("CheckDepositOrderCondition")]
        public IActionResult CheckDepositOrderCondition(CheckDepositOrderConditionRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.CheckDepositOrderCondition(request.Order);
                response.Result = saveResult.Id;
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(saveResult.ResultCode);
                response.Description = utils.GetActionResultErrors(saveResult.Errors);

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկի դեպքում գումարի չափը
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="loanCurrency"></param>
        /// <param name="provisionCurrency"></param>
        /// <returns></returns>
        [HttpPost("GetDepositLoanAndProvisionAmount")]
        public IActionResult GetDepositLoanAndProvisionAmount(DepositLoanAndProvisionAmountRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                double kursForLoan = _xbService.GetCBKursForDate(DateTime.Today.Date, request.LoanCurrency);
                double kursForProvision = _xbService.GetCBKursForDate(DateTime.Today.Date, request.ProvisionCurrency);
                response.Result = _xbService.GetDepositLoanAndProvisionAmount(request.Amount, request.LoanCurrency, request.ProvisionCurrency, kursForLoan, kursForProvision);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկի ամսական մարման չափը
        /// </summary>
        /// <param name="startCapital"></param>
        /// <param name="interestRate"></param>
        /// <param name="dateOfBeginning"></param>
        /// <param name="dateOfNormalEnd"></param>
        /// <param name="firstRepaymentDate"></param>
        /// <returns></returns>
        [HttpPost("GetRedemptionAmountForDepositLoan")]
        public IActionResult GetRedemptionAmountForDepositLoan(RedemptionAmountRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.Result = _xbService.GetRedemptionAmountForDepositLoan(request.StartCapital, request.InterestRate, request.DateOfBeginning, request.DateOfNormalEnd, request.FirstRepaymentDate);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկի միջնորդավճարը
        /// </summary>
        /// <param name="startCapital"></param>
        /// <param name="dateOfBeginning"></param>
        /// <param name="dateofNormalEnd"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        [HttpPost("GetCommisionAmountForDepositLoan")]
        public IActionResult GetCommisionAmountForDepositLoan(CommisionAmountRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.Result = _xbService.GetCommisionAmountForDepositLoan(request.StartCapital, request.DateOfBeginning, request.DateofNormalEnd, request.Currency);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ավանդի պայմանագրի տվյալները
        /// </summary>
        [HttpPost("GetDepositContract")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetDepositContract(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();

                response.Result = _xbService.PrintDepositContract(request.Id, false);

                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի գորոծող ավանդների մասին տեղեկատվությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetOpenedDeposits")]
        public IActionResult GetOpenedDeposits()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Deposit>> response = new SingleResponse<List<Deposit>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetDeposits(ProductQualityFilter.Opened);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        

    }
}