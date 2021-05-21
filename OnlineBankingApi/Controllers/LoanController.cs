using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class LoanController : ControllerBase
    {
        private readonly XBService _xbService;
        private readonly XBInfoService _xbInfoService;
        private readonly ContractManager _contractManager;
        private readonly CacheHelper _cacheHelper;

        public LoanController(XBService xbService, ContractManager contractManager, XBInfoService xbInfoService, CacheHelper cacheHelper)
        {
            _xbService = xbService;
            _xbInfoService = xbInfoService;
            _contractManager = contractManager;
            _cacheHelper = cacheHelper;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի վարկերի մասին տեղեկատվությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetLoans")]
        public IActionResult GetLoans(ProductQualityFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                var response = new SingleResponse<List<Loan>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetLoans(request.Filter);
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    response.Result.RemoveAll(m => !_xbService.HasProductPermissionByProductID((ulong)m.ProductId));
                }
                response.Result.RemoveAll(m => m.Quality == 10 && !m.Is_24_7);

                foreach (Loan loan in response.Result)
                {
                    if (loan.ContractDate != null)
                    {
                        loan.StartDate = loan.ContractDate ?? loan.StartDate;
                    }

                    if (loan.Is_24_7)
                    {
                        loan.CurrentCapital = loan.ContractAmount;
                    }
                }

                Parallel.ForEach(response.Result, x => x.HasContractFile = _xbService.HasUploadedContract(x.LoanAccount.AccountNumber,0));

                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ վարկի մասին տեղեկատվությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetLoan")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Loan })]
        public IActionResult GetLoan(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<Loan>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetLoan(request.ProductId);
                response.Result.HasContractFile = _xbService.HasUploadedContract(response.Result.LoanAccount.AccountNumber,0);

                List<ProductOtherFee> fees = new List<ProductOtherFee>();
                fees = _xbService.GetProductOtherFees(response.Result.ProductId).Where(x => x.Type == 1).ToList();

                if(fees.Count > 0)
                    response.Result.RepaymentAmount = fees[0].Amount;




                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Վերադարձնում է վարկի մարման գրաֆիկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetLoanRepaymentGrafik")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Loan })]
        public IActionResult GetLoanRepaymentGrafik(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<LoanRepaymentGrafik>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetLoanRepaymentGrafik(request.ProductId);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Պահպանում է վարկի մարման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveMatureOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveMatureOrder(MatureOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };
                if (request.Order.Type == OrderType.LoanMature)
                {
                    request.Order.Description = _xbInfoService.GetLoanMatureTypesForIBanking()
                                .Where(x => x.Key == ((int)request.Order.MatureType)
                                .ToString())
                                .First().Value; 
                }
                if(request.Order.MatureType == MatureType.PartialRepayment)
                {
                    request.Order.MatureType = MatureType.RepaymentByCreditCode;
                }
                ActionResult saveResult = _xbService.SaveMatureOrder(request.Order);

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
        /// Հաստատում/ուղարկում է վարկի մարման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveMatureOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.MatureOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveMatureOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                MatureOrder order = _cacheHelper.GetApprovalOrder<MatureOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveMatureOrder(order);

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
        /// Վերադարձնում է վարկի մայր գումարի մարման տուգանքի տվյալները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetLoanMatureCapitalPenalty")]
        public IActionResult GetLoanMatureCapitalPenalty(LoanMatureCapitalPenaltRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<double>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetLoanMatureCapitalPenalty(request.Order);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
         

        /// <summary>
        /// Պահպանում է ավանդի գրավով վարկային պրոդուկտի հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveLoanProductOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveLoanProductOrder(LoanProductOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveLoanProductOrder(request.Order);

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
        /// Հաստատում/ուղարկում է ավանդի գրավով վարկային պրոդուկտի հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveLoanProductOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.LoanProductOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]

        public IActionResult ApproveLoanProductOrder(ApproveLoanProductOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };
                LoanProductOrder order = new LoanProductOrder();
                AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();

                if (request.ProductType == 1)
                {
                    order = _xbService.GetLoanOrder(request.Id);
                }
                if (request.ProductType == 2)
                {
                    order = _xbService.GetCreditLineOrder(request.Id);
                }
                if (request.ProductType == 3)
                {
                    order = _xbService.GetCreditLineOrder(request.Id);                    
                }

                ActionResult saveResult = _xbService.ApproveLoanProductOrder(order);

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
        /// Վերադարձնում է տվյալ ավանդի գրավով վարկային պրոդուկտի տոկոսադրույքը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetLoanProductInterestRate")]
        public IActionResult GetLoanProductInterestRate(LoanProductInterestRateRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<double>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetLoanProductInterestRate(request.Order, request.CardNumber);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Իրականացնում է արագ օվերդրաֆտ ստանալու հայտի ստուգումներ տվյալ քարտի համար
        /// </summary>
        /// <returns></returns>
        [HttpPost("FastOverdraftValidations")]
        [TypeFilter(typeof(ValidateCardNumberFilter))]
        public IActionResult FastOverdraftValidations(CardNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                List<ActionError> result = _xbService.FastOverdraftValidations(request.CardNumber);

                if (result != null && result.Count > 0)
                {
                    response.ResultCode = ResultCodes.failed;
                    response.Description = utils.GetActionResultErrors(result);
                }
                else
                {
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
        /// Վերադարձնում է ավանդի գրավով վարկի հայտը
        /// </summary>
        /// <returns></returns>
        [TypeFilter(typeof(ValidateDocIdFilter))]
        [HttpPost("GetLoanOrder")]
        public IActionResult GetLoanOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<LoanProductOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetLoanOrder(request.Id);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        
     


        /// <summary>
        /// Վերադարձնում է տվյալ ժամանակահատվածում նշված վարկի քաղվածքը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetLoanStatement")]
        public IActionResult GetLoanStatement(LoanStatementRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<LoanStatement>() { ResultCode = ResultCodes.normal };
                Loan loan = _xbService.GetLoan(request.ProductId);
                response.Result = _xbService.GetLoanStatement(loan.LoanAccount.AccountNumber, request.DateFrom, request.DateTo, request.MinAmount, request.MaxAmount, request.DebCred, request.TransactionsCount, request.OrderByAscDesc);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        /// <summary>
        /// Վերադարձնում է վարկի մարման հայտի տվյալները
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetMatureOrder")]
        public IActionResult GetMatureOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xbService.GetMatureOrder(request.Id);
                order.SubType = 2;
                if (order.MatureType == MatureType.PartialRepayment)
                {
                    order.MatureType = MatureType.PartialRepaymentByGrafik;
                    order.SubTypeDescription = _xbInfoService.GetLoanMatureTypesForIBanking().Where(x => x.Key == "9").FirstOrDefault().Value;
                }
                else if (order.MatureType == MatureType.RepaymentByCreditCode) 
                {
                    order.MatureType = MatureType.PartialRepayment;
                    order.SubTypeDescription = _xbInfoService.GetLoanMatureTypesForIBanking().Where(x => x.Key == "2").FirstOrDefault().Value;
                } 
                else if (order.MatureType == MatureType.FullRepayment && order.Quality == OrderQuality.Draft)
                {
                    order.MatureType = MatureType.PartialRepayment;
                    order.SubTypeDescription = _xbInfoService.GetLoanMatureTypesForIBanking().Where(x => x.Key == "2").FirstOrDefault().Value;
                }
                SingleResponse<MatureOrder> response = new SingleResponse<MatureOrder>();
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    if (!_xbService.HasProductPermission(order.Account.AccountNumber))
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
        /// Վերադարձնում է ճիշտ, եթե հաճախորդը կարող է դիմել վարկի համար։
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost("IsAbleToApplyForLoan")]
        public IActionResult IsAbleToApplyForLoan()
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<dynamic>() { ResultCode = ResultCodes.normal };
                var loan = _xbService.IsAbleToApplyForLoan(LoanProductType.Loan);
                var creditline = _xbService.IsAbleToApplyForLoan(LoanProductType.CreditLine);
                var fastOverdraft = _xbService.IsAbleToApplyForLoan(LoanProductType.FastOverdraft);
                response.Result = new { Loan = loan, CreditLine = creditline, FastOverdraft = fastOverdraft };
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }

        }


        /// <summary>
        /// Վերադարձնում է գռավադրվող գումարի դիմաց վարկային գծին տրամադրվող հասանելի մնացորդը ։
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetMaxAvailableAmountForNewCreditLine")]
        public IActionResult GetMaxAvailableAmountForNewCreditLine(GetMaxAvailableAmountForNewCreditLine request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>() { ResultCode = ResultCodes.normal };
                XBS.AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                response.Result = _xbService.GetMaxAvailableAmountForNewCreditLine(request.ProductId, request.CreditLineType, request.ProvisionCurrency, request.ExistRequiredEntries, authorizedCustomer.CustomerNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        ///<summary>
        ///Վերադարձնում է գրավադրվող գումարի դիմաց վարկին տրամադրվող հասանելի մնացորդը։
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetMaxAvailableAmountForNewLoan")]
        public IActionResult GetMaxAvailableAmountForNewLoan(CurrencyRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>() { ResultCode = ResultCodes.normal };
                XBS.AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                response.Result = _xbService.GetMaxAvailableAmountForNewLoan(request.Currency, authorizedCustomer.CustomerNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


    }
}