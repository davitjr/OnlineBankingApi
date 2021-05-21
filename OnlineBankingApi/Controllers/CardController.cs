using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Resources;
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
    public class CardController : Controller
    {
        private readonly XBService _xbService;
        private readonly XBInfoService _xBInfoService;
        private readonly CacheHelper _cacheHelper;
        private readonly IConfiguration _config;
        private readonly IStringLocalizer _localizer;

        public CardController(XBService xbService, CacheHelper cacheHelper, XBInfoService xBInfoService, IConfiguration config, IStringLocalizer<SharedResource> localizer)
        {
            _xbService = xbService;
            _cacheHelper = cacheHelper;
            _xBInfoService = xBInfoService;
            _config = config;
            _localizer = localizer;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի բոլոր քարտերի մասին տեղեկատվությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCards")]
        public IActionResult GetCards(ProductQualityFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();

                SingleResponse<List<Card>> response = new SingleResponse<List<Card>>() { ResultCode = ResultCodes.normal };
                List<Card> cards = _xbService.GetCards(request.Filter);
                if (request.Filter == ProductQualityFilter.Closed)
                {
                    cards = _xbService.GetClosedCardsForDigitalBanking();
                }
                else
                {
                    cards = _xbService.GetCards(request.Filter);
                    if (request.Filter == ProductQualityFilter.All)
                    {
                        cards.RemoveAll(m => m.ClosingDate != null);
                        cards.AddRange(_xbService.GetClosedCardsForDigitalBanking());
                        cards.AddRange(_xbService.GetnotActivatedVirtualCards());
                    }
                    cards.AddRange(_xbService.GetLinkedCards());
                }

                foreach (var card in cards)
                {
                    if (card.CardNumber != null)
                    {
                        card.ArCaBalance = _xbService.GetArcaBalance(card.CardNumber);
                        card.CardAccount.ArcaBalance = card.ArCaBalance;
                        if (card.CardAccount.AccountNumber == "220000295380000" && Convert.ToBoolean(_config["TestVersion"]))
                            card.ArCaBalance = null;
                        card.CVVNote = _xbService.GetCVVNote((ulong)card.ProductId);
                        card.FilialName = _xBInfoService.GetFilialName(card.FilialCode);
                        card.ProductNote = _xbService.GetProductNote(card.ProductId);
                        if (card.Type == 51)  //VISA VIRTUAL
                        {
                            card.RealCVV = _xbService.GetVirtualCardDetails(card.CardNumber)?.Cvv;
                        }
                    }
                }
                response.Result = cards;
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    response.Result.RemoveAll(m => !_xbService.HasProductPermission(m.CardAccount.AccountNumber, (ulong)m.ProductId));
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ քարտի մասին տեղեկատվությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCard")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Card })]
        public IActionResult GetCard(ProductIdRequest request)
        {
            
            if (ModelState.IsValid)
            {

                SingleResponse<Card> response = new SingleResponse<Card>() { ResultCode = ResultCodes.normal };
                Card card = new Card();
                card = _xbService.GetCard(request.ProductId);
                card.CardStatus = _xbService.GetCardStatus(request.ProductId);
                if (card.CardStatus.StatusDescription == "ՏՐ")
                    card.CardStatus.StatusDescription = _localizer["Տրամադրված"];
                else
                    card.CardStatus.StatusDescription = _localizer["Չտրամադրված"];

                card.Password = _xbService.GetCardMotherName(request.ProductId);
                CardAdditionalInfo addInfo = _xbService.GetCardAdditionalInfo(request.ProductId);
                card.CardSMSPhone = addInfo.SMSPhone;
                card.ReportReceivingType = addInfo.ReportReceivingType;
                card.CardEmail = addInfo.Email;
                card.ArCaBalance = _xbService.GetArcaBalance(card.CardNumber);
                card.CardAccount.ArcaBalance = card.ArCaBalance;
                if (card.CardAccount.AccountNumber == "220000295380000" && Convert.ToBoolean(_config["TestVersion"]))
                    card.ArCaBalance = null;
                if (card.Type == 34 || card.Type == 40)
                {
                    card.CashBack = _xbService.GetCashBackAmount(request.ProductId);
                }
                if (card.Type == 41 || card.Type == 40)
                {
                    var MR = _xbService.GetCardMembershipRewards(card.CardNumber);
                    if (MR == null)
                        card.BonusBalance = 0;
                    else
                        card.BonusBalance = MR.BonusBalance;
                }
                card.FilialName = _xBInfoService.GetFilialName(card.FilialCode);              
                response.Result = card;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ ժամանակահատվածում նշված քարտի քաղվածքը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardStatement")]
        public IActionResult GetCardStatement(CardStatementRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<CardStatement>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCardStatement(request.CardNumber, request.DateFrom, request.DateTo, request.MinAmount, request.MaxAmount, request.DebCred,
                    request.TransactionsCount, request.OrderByAscDesc);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ քարտի մնացորդը ARCA համակարգում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetArcaBalance")]
        public IActionResult GetArcaBalance(CardNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double?> response = new SingleResponse<double?>() { ResultCode = ResultCodes.normal };
                double? result = _xbService.GetArcaBalance(request.CardNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Վերադարձնում է քարտի MR ծրագրին անդամակցության տվյալները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardMembershipRewards")]
        public IActionResult GetCardMembershipRewards(CardNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<MembershipRewards>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCardMembershipRewards(request.CardNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }



        /// <summary>
        /// Վերադարձնում է քարտի փակման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardClosingOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetCardClosingOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<CardClosingOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCardClosingOrder(request.Id);
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
        ///Քարտի փակման հայտի պահպանում
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveCardClosingOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveCardClosingOrder(CardClosingOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveCardClosingOrder(request.Order);

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
        /// Քարտի փակման հայտի ուղարկում
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveCardClosingOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.CardClosingOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveCardClosingOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                CardClosingOrder order = _cacheHelper.GetApprovalOrder<CardClosingOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveCardClosingOrder(order);

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
        ///Քարտի բլոկավորման/ապաբլոկավորման հայտի պահպանում
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveArcaCardsTransactionOrder")]
        public IActionResult SaveArcaCardsTransactionOrder(ArcaCardsTransactionOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveArcaCardsTransactionOrder(request.Order);

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
        /// Վերադարձնում է քարտի բլոկավորման/ապաբլոկավորման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetArcaCardsTransactionOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetArcaCardsTransactionOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<ArcaCardsTransactionOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetArcaCardsTransactionOrder(request.Id);
                response.Result.ProductId = _xbService.GetCardProductId(response.Result.CardNumber, response.Result.CustomerNumber);
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
        /// Քարտի բլոկավորման/ապաբլոկավորման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveArcaCardsTransactionOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.ArcaCardsTransactionOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveArcaCardsTransactionOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ArcaCardsTransactionOrder order = _cacheHelper.GetApprovalOrder<ArcaCardsTransactionOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveArcaCardsTransactionOrder(order);

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
        /// Վերադարձնում է քարտի վրա գրված հաճախորդի անունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetEmbossingName")]
        public IActionResult GetEmbossingName(CardNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<string>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetEmbossingName(request.CardNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քարտի լիմիտները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardLimits")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Card })]
        public IActionResult GetCardLimits(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<KeyValuePair<string, string>>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCardLimits((long)request.ProductId);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Վերադարձնում է քարտի լիմիտների փոփոխության հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardLimitChangeOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetCardLimitChangeOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<CardLimitChangeOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCardLimitChangeOrder(request.Id);
                response.Result.ProductId = _xbService.GetCardProductId(response.Result.CardNumber, response.Result.CustomerNumber);
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
        /// Քարտի լիմիտների փոփոխության հայտի պահպանում և ուղարկում
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveCardLimitChangeOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.CardLimitChangeOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveCardLimitChangeOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                CardLimitChangeOrder order = _cacheHelper.GetApprovalOrder<CardLimitChangeOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveCardLimitChangeOrder(order);

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
        /// Պահպանում է քարտի լիմիտների փոփոխության հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveCardLimitChangeOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveCardLimitChangeOrder(CardLimitChangeOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveCardLimitChangeOrder(request.Order);

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
        /// Նոր քարտի պատվիրման հայտի ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPlasticCardOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetPlasticCardOrder(OrderIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<PlasticCardOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetPlasticCardOrder(request.OrderId);
                if (response.Result.ServiceFeePeriodicityType == (ServiceFeePeriodicityType)_xBInfoService.GetServiceFeePeriodocityTypes()[0].Key)
                {
                    response.Result.ServiceFeePeriodicityTypeDescription = _xBInfoService.GetServiceFeePeriodocityTypes()[0].Value;
                }
                else
                {
                    response.Result.ServiceFeePeriodicityTypeDescription = _xBInfoService.GetServiceFeePeriodocityTypes()[1].Value;
                }
                if (response.Result.Quality == OrderQuality.Declined)
                {
                    response.Result.RejectReason = _xbService.GetOrderRejectReason(request.OrderId, response.Result.Type);
                }
                response.Result.PlasticCard.ProductId = (ulong)_xbService.GetCardProductId(response.Result.PlasticCard.MainCardNumber, response.Result.CustomerNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Հաստատում/ուղարկում է պլաստիկ քարտի հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApprovePlasticCardOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.PlasticCardOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApprovePlasticCardOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                PlasticCardOrder order = _cacheHelper.GetApprovalOrder<PlasticCardOrder>(request.Id);
                if (order.Attachments != null)
                {
                    foreach (var item in order.Attachments)
                    {
                        item.AttachmentInBase64 = _xbService.GetOrderAttachmentInBase64(item.Id);
                    }
                }
                ActionResult saveResult = _xbService.ApprovePlasticCardOrder(order);

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
        /// Պահպանում է պլաստիկ քարտի հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SavePlasticCardOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SavePlasticCardOrder(PlasticCardOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SavePlasticCardOrder(request.Order);

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
        /// Վերադարձնում է քարտը՝ ըստ հաշվեհամարի
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardByAccountNumber")]
        [TypeFilter(typeof(ValidateAccountNumberFilter))]
        public IActionResult GetCardByAccountNumber(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<Card> response = new SingleResponse<Card>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCardByAccountNumber(request.AccountNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ամբողջական քարտի GET
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetFullCardByAccountNumber")]
        [TypeFilter(typeof(ValidateAccountNumberFilter))]
        public IActionResult GetFullCardByAccountNumber(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<Card> response = new SingleResponse<Card>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCardByAccountNumber(request.AccountNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քարտի գերածախսի տվյալները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardOverdraft")]
        public IActionResult GetCardOverdraft(CardNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<CreditLine> response = new SingleResponse<CreditLine>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCardOverdraft(request.CardNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// AMEX_MR ի սպասարկման գումար
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetMRFeeAMD")]
        public IActionResult GetMRFeeAMD(CardNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetMRFeeAMD(request.CardNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է վարկային գիծ չունեցող քարտերը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAvailableCardsForCreditLine")]
        public IActionResult GetAvailableCardsForCreditLine(ProductQualityFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                var response = new SingleResponse<List<Card>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetAvailableCardsForCreditLine(request.Filter);
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    response.Result.RemoveAll(m => !_xbService.HasProductPermission(m.CardAccount.AccountNumber, (ulong)m.ProductId));
                }
                foreach (var card in response.Result)
                {
                    if (card.CardNumber != null)
                    {
                        card.ArCaBalance = _xbService.GetArcaBalance(card.CardNumber);
                        if (card.CardAccount.AccountNumber == "220000295380000" && Convert.ToBoolean(_config["TestVersion"]))
                            card.ArCaBalance = null;

                    }
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Քարտի տեսակի ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardTypeName")]
        public IActionResult GetCardTypeName(CardNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<string>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCardTypeName(request.CardNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի քարտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardByCardNumber")]
        [TypeFilter(typeof(ValidateCardNumberFilter))]
        public IActionResult GetCardByCardNumber(CardNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<Card> response = new SingleResponse<Card>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCardByCardNumber(request.CardNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// վերադարձնում է քարտի կից և լրացուցիչ քարտերը 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="productFilter"></param>
        /// <returns></returns>
        [HttpPost("GetLinkedAndAttachedCards")]
        public IActionResult GetLinkedAndAttachedCards(LinkedAndAttachedCardsRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Card>> response = new SingleResponse<List<Card>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetLinkedAndAttachedCards(request.ProductId, request.ProductFilter);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի գործող քարտերի մասին տեղեկատվությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetOpenedCards")]
        public IActionResult GetOpenedCards()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Card>> response = new SingleResponse<List<Card>>() { ResultCode = ResultCodes.normal };
                List<Card> cards = _xbService.GetCards(ProductQualityFilter.Opened);
                cards.AddRange(_xbService.GetLinkedCards());

                foreach (var card in cards)
                {
                    card.ArCaBalance = _xbService.GetArcaBalance(card.CardNumber);
                    if (card.CardAccount.AccountNumber == "220000295380000" && Convert.ToBoolean(_config["TestVersion"]))
                        card.ArCaBalance = null;
                    card.CVVNote = _xbService.GetCVVNote((ulong)card.ProductId);
                }
                response.Result = cards;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ քարտի սակագները
        /// </summary>
        /// <param name="productIdRequest"></param>
        /// <returns></returns>
        [HttpPost("GetCardTariff")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Card })]
        public IActionResult GetCardTariff(ProductIdRequest productIdRequest)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<CardTariff> response = new SingleResponse<CardTariff>() { ResultCode = ResultCodes.normal };
                CardTariff cardTariff = _xbService.GetCardTariff(productIdRequest.ProductId);
                response.Result = cardTariff;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Պահպանում է հաճախորդի կողմից մուտքագրված CVV-ի վերաբերյալ նշումը
        /// </summary>
        /// <param name="CVVNoteRequest"></param>
        /// <returns></returns>
        [HttpPost("SaveCVVNote")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Card })]
        public IActionResult SaveCVVNote(CVVNoteRequest CVVNoteRequest)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                response.ResultCode = ResultCodes.normal;
                XBS.ActionResult result = _xbService.SaveCVVNote(CVVNoteRequest.ProductId, CVVNoteRequest.CVVNote);
                response.Result = result.Id;
                response.Description = Utils.GetActionResultErrors(result.Errors);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        /// <summary>
        /// Վերադարձնում է վիրտուալ քարտի մանրամասն տվյալները
        /// </summary>
        /// <param name="virtualCardDetails"></param>
        /// <returns></returns>
        [HttpPost("GetVirtualCardDetails")]
        public IActionResult GetVirtualCardDetails(CardNumberRequest virtualCardDetails)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<VirtualCardDetails>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetVirtualCardDetails(virtualCardDetails.CardNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        /// <summary>
        ///  Ակտիվացնում է քարտը ԱՌՔԱ ում, բացում է քարտի քարտային հաշիվները
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        [HttpPost("ActivateAndOpenProductAccounts")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.CardActivationOrder })]
        [TypeFilter(typeof(SeconfConfirmationFilter))]

        public IActionResult ActivateAndOpenProductAccounts(ProductIdApproveRequest productID)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.ActivateAndOpenProductAccounts(productID.ProductId);

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
        /// Քարտի  SMS ծառայության ակտիվացման, դադարեցման կամ փոփոխման հայտի ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPlasticCardSMSServiceOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetPlasticCardSMSServiceOrder(OrderIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<PlasticCardSMSServiceOrder>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetPlasticCardSMSServiceOrder(request.OrderId);
                if (response.Result.Quality == OrderQuality.Declined)
                {
                    response.Result.RejectReason = _xbService.GetOrderRejectReason(request.OrderId, response.Result.Type);
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }



        /// <summary>
        /// Պահպանում է  պլաստիկ քարտի SMS ծառայության գրանցման,փոփոխման կամ դադարեցման  հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("SavePlasticCardSMSServiceOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SavePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SavePlasticCardSMSServiceOrder(request.Order);

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
        /// Հաստատում է  պլաստիկ քարտի SMS ծառայության գրանցման,փոփոխման կամ դադարեցման  հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApprovePlasticCardSMSServiceOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.PlasticCardSmsServiceOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApprovePlasticCardSMSServiceOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                PlasticCardSMSServiceOrder order = _cacheHelper.GetApprovalOrder<PlasticCardSMSServiceOrder>(request.Id);
                ActionResult saveResult = _xbService.ApprovePlasticCardSMSServiceOrder(order);

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
        /// Վերադարձնում է տվյալ քարտի  SMS-ի սակագինը
        /// </summary>
        /// <param name="productIdRequest"></param>
        /// <returns></returns>
        [HttpPost("GetPlasticCardSMSServiceTariff")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Card })]
        public IActionResult GetPlasticCardSMSServiceTariff(ProductIdRequest productIdRequest)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<float> response = new SingleResponse<float>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetPlasticCardSMSServiceTariff(productIdRequest.ProductId)[1];
                 return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քարտի SMS ծառայության նախորդ տվյալները
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetPlasticCardSMSServiceHistory")]
        public IActionResult GetPlasticCardSMSServiceHistory(CardNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<PlasticCardSMSServiceHistory> response = new SingleResponse<PlasticCardSMSServiceHistory>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetPlasticCardSMSServiceHistory(request.CardNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// վերադարձնում է քարտից քարտ փոխանցման համար ելքագրվող քարտերի ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardsForC2C")]
        public IActionResult GetCardsForC2C()
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                SingleResponse<List<Card>> response = new SingleResponse<List<Card>>() { ResultCode = ResultCodes.normal };
                var cards = _xbService.GetCards(ProductQualityFilter.Opened);
                cards.RemoveAll(x => x.SupplementaryType == SupplementaryType.Linked);
                cards.AddRange(_xbService.GetLinkedCards());
                foreach (var card in cards)
                {
                    if (card.CardNumber != null)
                    {
                        card.ArCaBalance = _xbService.GetArcaBalance(card.CardNumber);
                        card.CardAccount.ArcaBalance = card.ArCaBalance;
                        if (card.CardAccount.AccountNumber == "220000295380000" && Convert.ToBoolean(_config["TestVersion"]))
                            card.ArCaBalance = null;
                        card.CVVNote = _xbService.GetCVVNote((ulong)card.ProductId);
                        card.FilialName = _xBInfoService.GetFilialName(card.FilialCode);
                    }
                }
                response.Result = cards;
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    response.Result.RemoveAll(m => !_xbService.HasProductPermission(m.CardAccount.AccountNumber, (ulong)m.ProductId));
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

    }
}