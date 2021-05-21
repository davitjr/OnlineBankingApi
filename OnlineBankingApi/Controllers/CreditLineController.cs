using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class CreditLineController : ControllerBase
    {
        private readonly XBService _xbService;
        private readonly XBInfoService _xbInfoService;
        private readonly CacheHelper _cacheHelper;
        private readonly IConfiguration _config;

        public CreditLineController(XBService xbService,XBInfoService xbInfoService, CacheHelper cacheHelper, IConfiguration config)
        {
            _xbService = xbService;
            _xbInfoService = xbInfoService;
            _cacheHelper = cacheHelper;
            _config = config;
        }


        /// <summary>
        /// Վերադարձնում է տվյալ վարկային գծի տվյալները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCreditLine")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.CreditLine })]
        public IActionResult GetCreditLine(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<CreditLine>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCreditLine(request.ProductId);
                response.Result.HasContractFile = _xbService.HasUploadedContract(response.Result.LoanAccount.AccountNumber, 1);
                List<CreditLineGrafik> grafik = _xbService.GetCreditLineGrafik((ulong)response.Result.ProductId);
                if (grafik.Count != 0)
                {
                    grafik.Sort((x, y) => x.EndDate.CompareTo(y.EndDate));
                    var selectedRow = grafik.Find(x => x.EndDate >= _xbService.GetNextOperDay() && x.Amount - x.MaturedAmount > 0);

                    if (selectedRow != null)
                    {
                        response.Result.NextRepaymentAmount = selectedRow.Amount - selectedRow.MaturedAmount;
                        response.Result.NextRepaymentDate = selectedRow.EndDate;
                    }
                }
                //վարտային վարկային քիժ
                if (response.Result.ConnectAccount.AccountType == 11)
                {
                    response.Result.CardSystem = _xbService.GetCardByCardNumber(response.Result.CardNumber).CardSystem;
                }
                //Քարտային վարկային գիծ
                if(response.Result.ConnectAccount.AccountType == 11)
                {
                    response.Result.LoanAccount.CardSystem = _xbService.GetCardByCardNumber(response.Result.LoanAccount.ProductNumber).CardSystem;
                }

                response.Result.TotalDebt = Math.Abs(response.Result.Quality == 11 || response.Result.Quality == 12 ? response.Result.OutCapital : response.Result.CurrentCapital)
                   + Math.Abs(response.Result.CurrentRateValue) + Math.Abs(response.Result.PenaltyAdd) + response.Result.JudgmentRate;
                if ((!string.IsNullOrWhiteSpace(response.Result.CardNumber)) && response.Result.CardNumber != "0")
                {
                    CreditLine overdraft = _xbService.GetCardOverDraft(response.Result.CardNumber);
                    response.Result.CardOverdraft = Math.Abs(overdraft.Quality == 11 || overdraft.Quality == 12 ? overdraft.OutCapital : overdraft.CurrentCapital)
                    + Math.Abs(overdraft.CurrentRateValue) + Math.Abs(overdraft.PenaltyAdd) + overdraft.JudgmentRate;
                    response.Result.TotalDebt += response.Result.CardOverdraft;
                }  
                
                response.Result.PenaltyAdd -= response.Result.JudgmentRate;
                    
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ վարկային գծի գրաֆիկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCreditLineGrafik")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.CreditLine })]
        public IActionResult GetCreditLineGrafik(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<CreditLineGrafik>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCreditLineGrafik(request.ProductId);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ վարկային գծի հայտի մանրամասները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCreditLineOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetCreditLineOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<LoanProductOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCreditLineOrder(request.Id);
                response.Result.ProductId = _xbService.GetCardProductIdByAccountNumber(response.Result.ProductAccount.AccountNumber, _cacheHelper.GetAuthorizedCustomer().CustomerNumber);
                response.Result.ProductCardNumber = _xbService.GetCardByAccountNumber(response.Result.ProductAccount.AccountNumber).CardNumber;
                response.Result.ProductCardSystem = _xbService.GetCardSystem(response.Result.ProductCardNumber);
                response.Result.MandatoryPaymentDescription = _xbInfoService.GetCreditLineMandatoryInstallmentTypes().Where(x => x.Key == (response.Result.MandatoryPayment == true ? "1" : "0")).FirstOrDefault().Value;
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
        /// Վերադարձնում է վարկային գծի դադարեցման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCreditLineTerminationOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetCreditLineTerminationOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<CreditLineTerminationOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCreditLineTerminationOrder(request.Id);
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
        /// Պահպանում է վարկային գծի դադարեցման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveCreditLineTerminationOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveCreditLineTerminationOrder(CreditLineTerminationOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveCreditLineTerminationOrder(request.Order);

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
        /// Վերադարձնում է տվյալ հաշվեհամարին համապատասխան վարկային գծի տվյալները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCreditLineByAccountNumber")]
        public IActionResult GetCreditLineByAccountNumber(LoanFullNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<CreditLine> response = new SingleResponse<CreditLine>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCreditLineByAccountNumber(request.LoanFullNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ վարկային գծի հաջորդ վճարման մանրամասները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCreditLineRepayment")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.CreditLine })]
        public IActionResult GetCreditLineRepayment(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<CreditLineGrafik>> response = new SingleResponse<List<CreditLineGrafik>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCreditLineRepayment(request.ProductId);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Հաստատում է վարկային գծի դադարեցման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveCreditLineTerminationOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.CreditLineTerminationOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveCreditLineTerminationOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                CreditLineTerminationOrder order = _cacheHelper.GetApprovalOrder<CreditLineTerminationOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveCreditLineTerminationOrder(order);

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
        /// Վերադարձնում է վարկային գիծի վերջին օրը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCreditLineLastDate")]
        public IActionResult GetCreditLineLastDate(CreditLineLastDateRequest request)
        {

            if (ModelState.IsValid)
            {
                var response = new SingleResponse<DateTime?>() { ResultCode = ResultCodes.normal };

                var nextOperDay = _xbService.GetNextOperDay();
                List<Card> availableCards = _xbService.GetCardsForNewCreditLine(OrderType.CreditLineSecureDeposit);

                if (request.CreditLineType == 50 || request.CreditLineType == 51)
                {

                    response.Result = availableCards.Find(x => x.CardNumber == request.CardNumber).ValidationDate;

                    if (nextOperDay.AddMonths(60) < response.Result)
                    {
                        response.Result = nextOperDay.AddMonths(60);
                    }

                    response.ResultCode = ResultCodes.normal;
                }
                else if (request.CreditLineType == 30)
                {
                    response.Result = availableCards.Find(x => x.CardNumber == request.CardNumber).EndDate;

                    if (nextOperDay.AddMonths(36) < response.Result)
                    {
                        response.Result = nextOperDay.AddMonths(36);
                    }

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
        /// Վերադարձնում է ավանդի գրավով վարկային գծի դեպքում գումարի չափը
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="loanCurrency"></param>
        /// <param name="provisionCurrency"></param>
        /// <param name="mandatoryPayment"></param>
        /// <param name="creditLineType"></param>
        /// <returns></returns>
        [HttpPost("GetCreditLineProvisionAmount")]
        public IActionResult GetCreditLineProvisionAmount(CreditLineProvisionAmountRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                double kursForLoan = _xbService.GetCBKursForDate(DateTime.Today.Date, request.LoanCurrency);
                double kursForProvision = _xbService.GetCBKursForDate(DateTime.Today.Date, request.ProvisionCurrency);
                response.Result = _xbService.GetCreditLineProvisionAmount(request.Amount, request.LoanCurrency, request.ProvisionCurrency,
                    request.MandatoryPayment, request.CreditLineType, kursForLoan, kursForProvision);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի բոլոր վարկային գծերը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetCreditLines")]
        public IActionResult GetCreditLines(ProductQualityFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<CreditLine>> response = new SingleResponse<List<CreditLine>>();
                response.Result = _xbService.GetCreditLines(request.Filter);
                response.Result.RemoveAll(m => m.Type == 25 || m.Type == 18 || m.Type == 46 || m.Type == 36 || m.Type == 60);
                Parallel.ForEach(response.Result, x => x.HasContractFile = _xbService.HasUploadedContract(x.LoanAccount.AccountNumber, 1));
                Parallel.ForEach(response.Result, item =>
                {
                    List<CreditLineGrafik> grafik = _xbService.GetCreditLineGrafik((ulong)item.ProductId);
                    if(grafik.Count != 0)
                    {
                        grafik.Sort((x, y) => x.EndDate.CompareTo(y.EndDate));
                        var selectedRow = grafik.Find(x => x.EndDate >= _xbService.GetNextOperDay() && x.Amount - x.MaturedAmount > 0);
                            
                        if(selectedRow != null)
                        {
                            item.NextRepaymentAmount = selectedRow.Amount - selectedRow.MaturedAmount;
                            item.NextRepaymentDate = selectedRow.EndDate;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(item.CardNumber))
                    {
                        item.CardSystem = _xbService.GetCardSystem(item.CardNumber);
                    }
                    if (!string.IsNullOrWhiteSpace(item.LoanAccount.ProductNumber))
                    {
                        item.LoanAccount.CardSystem = _xbService.GetCardSystem(item.LoanAccount.ProductNumber);
                    }

                    item.TotalDebt = Math.Abs(item.Quality == 11 || item.Quality == 12 ? item.OutCapital : item.CurrentCapital)
                   + Math.Abs(item.CurrentRateValue) + Math.Abs(item.PenaltyAdd) + item.JudgmentRate;
                    if ((!string.IsNullOrWhiteSpace(item.CardNumber)) && item.CardNumber != "0")
                    {
                        CreditLine overdraft = _xbService.GetCardOverDraft(item.CardNumber);
                        item.CardOverdraft = Math.Abs(overdraft.Quality == 11 || overdraft.Quality == 12 ? overdraft.OutCapital : overdraft.CurrentCapital)
                        + Math.Abs(overdraft.CurrentRateValue) + Math.Abs(overdraft.PenaltyAdd) + overdraft.JudgmentRate;
                        item.TotalDebt += item.CardOverdraft;
                    }

                    item.PenaltyAdd -= item.JudgmentRate;
                });
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ ժամանակահատվածում նշված վարկային գծի քաղվածքը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetCreditLineStatement")]
        public IActionResult GetCreditLineStatement(LoanStatementRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<LoanStatement>() { ResultCode = ResultCodes.normal };
                CreditLine creditLine = _xbService.GetCreditLine(request.ProductId);
                response.Result = _xbService.GetLoanStatement(creditLine.LoanAccount.AccountNumber, request.DateFrom, request.DateTo, request.MinAmount, request.MaxAmount, request.DebCred, request.TransactionsCount, request.OrderByAscDesc);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է իրավաբանական անձանց դեպքում ընթացիկ հաշվի վարկային գծերը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetCurrentAccountOverdrafts")]
        public IActionResult GetCurrentAccountOverdrafts(ProductQualityFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<CreditLine>> response = new SingleResponse<List<CreditLine>>();
                response.Result = _xbService.GetCreditLines(request.Filter);
                response.Result = response.Result.FindAll(m => m.Type == 25 || m.Type == 18 || m.Type == 46 || m.Type == 36 || m.Type == 60);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է նոր վարկային գծի համար հասանելի քարտերի ցանկը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetCardsForNewCreditLine")]
        public IActionResult GetCardsForNewCreditLine(OrderTypeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Card>> response = new SingleResponse<List<Card>>();
                response.Result = _xbService.GetCardsForNewCreditLine(request.OrderType);
                foreach (var card in response.Result)
                {
                    if (card.CardNumber != null)
                    {
                        card.ArCaBalance = _xbService.GetArcaBalance(card.CardNumber);
                        if (card.CardAccount.AccountNumber == "220000295380000" && Convert.ToBoolean(_config["TestVersion"]))
                            card.ArCaBalance = null;
                       
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



    }
}