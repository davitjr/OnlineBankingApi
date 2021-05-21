using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Services;
using XBS;
using static OnlineBankingApi.Enumerations;
using ResponseExtensions = OnlineBankingApi.Models.ResponseExtensions;
using ActionResult = XBS.ActionResult;
using utils = OnlineBankingLibrary.Utilities.Utils;
using OnlineBankingLibrary.Models;
using OnlineBankingApi.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using OnlineBankingLibrary;
using OnlineBankingLibrary.Utilities;
using Microsoft.Extensions.Configuration;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class AccountController : ControllerBase
    {
        private readonly XBService _xbService;
        private readonly ContractService _contractService;
        private readonly CacheHelper _cacheHelper;
        private readonly IConfiguration _config;
        public AccountController(XBService xbService, ContractService contractService, CacheHelper cacheHelper, IConfiguration config)
        {
            _xbService = xbService;
            _contractService = contractService;
            _cacheHelper = cacheHelper;
            _config = config;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի բոլոր հաշիվների մասին տեղեկատվությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAccounts")]
        public IActionResult GetAccounts()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Account>> response = new SingleResponse<List<Account>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xbService.GetAccounts();
                Parallel.ForEach(response.Result, x => x.HasContractFile = _xbService.HasUploadedContract(x.AccountNumber, 3));
                Parallel.ForEach(response.Result, m =>
                {
                    if (m.AccountType == 11)
                    {
                        m.ArcaBalance = _xbService.GetArcaBalance(m.AccountDescription.Substring(0, 16).Trim());
                        if (m.AccountNumber == "220000295380000" && Convert.ToBoolean(_config["TestVersion"]))
                            m.ArcaBalance = null;
                    }
                });
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաշվի մանրամասները
        /// </summary>
        /// <param name="request">Հաշվեհամար</param>
        /// <returns></returns>
        [HttpPost("GetAccount")]
        [TypeFilter(typeof(ValidateAccountNumberFilter))]
        public IActionResult GetAccount(AccountNumberRequest request)
        {

            if (ModelState.IsValid)
            {
                SingleResponse<Account> response = new SingleResponse<Account>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetAccount(request.AccountNumber);
                response.Result.HasContractFile = _xbService.HasUploadedContract(response.Result.AccountNumber, 3);
                var overdraft = _xbService.GetCreditLines(ProductQualityFilter.Opened)
                    .Where(x => x.ConnectAccount.AccountNumber == response.Result.AccountNumber).ToList();
                response.Result.Overdraft = overdraft.FirstOrDefault();
                foreach (var item in overdraft)
                {
                    if(item.Type == 25)
                    {
                        response.Result.Overdraft = item;
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
        /// Վերադարձնում է հաճախորդի բոլոր ընթացիկ հաշիվների մասին տեղեկատվությունը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("GetCurrentAccounts")]
        public IActionResult GetCurrentAccounts(ProductQualityFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                SingleResponse<List<Account>> response = new SingleResponse<List<Account>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetCurrentAccounts(request.Filter);
                if(authorizedCustomer.LimitedAccess != 0)
                {
                    response.Result.RemoveAll(m => !_xbService.HasProductPermission(m.AccountNumber));
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Վերադարձնում է հաշվի քաղվածքը ֆիլտրներին համապատասխան
        /// </summary>
        /// <param name="accountNumber">Հաշվեհամար</param>
        /// <param name="dateFrom">Քաղվածքի սկիզբ</param>
        /// <param name="dateTo">Քաղվածքի վերջ</param>
        /// <param name="minAmount">Մուտքագրվող մինիմալ գումար  գործարքները ֆիլտրելու համար(default parameter = -1)</param>
        /// <param name="maxAmount">Մուտքագրվող մաքսիմալ գումար  գործարքները ֆիլտրելու համար(default parameter = -1)</param>
        /// <param name="debCred">Դեբեդ /կրեդիտ եթե փոխանցվում է с միայն կրեդիտներն է  , d միայն դեբետները ,  null բոլորը(default parameter = null)</param>
        /// <param name="transactionsCount">Ցուցադրվող գործարքների քանակ(default parameter = 0)</param>
        /// <param name="orderByAscDesc">Գործարքների խմբավորում ըստ գումարի աճման/նվազման կարգով ։ Եթե 1 աճման , 2 նվազման, 0 առանց խմբավորման(default parameter = 0)</param>
        /// <returns></returns>
        [HttpPost("GetAccountStatement")]
        public IActionResult GetAccountStatement(AccountStatementRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<AccountStatement>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetAccountStatement(request.AccountNumber, request.DateFrom, request.DateTo, request.MinAmount,
                    request.MaxAmount, request.DebCred, request.TransactionsCount, request.OrderByAscDesc);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }

        }


        /// <summary>
        /// Վերադարձնում է տվյալ հաշվեհամարի նկարագրությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAccountDescription")]
        public IActionResult GetAccountDescription(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetAccountDescription(request.AccountNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }

        }


        /// <summary>
        /// Ընթացիկ հաշվի փակման հայտի պահպանում
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveAccountClosingOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveAccountClosingOrder(AccountClosingOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveAccountClosingOrder(request.Order);

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
        /// Ընթացիկ հաշվի փակման հայտի ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAccountClosingOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetAccountClosingOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xbService.GetAccountClosingOrder(request.Id);
                var response = new SingleResponse<AccountClosingOrder>();
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
        /// Ընթացիկ հաշվի փակման հայտի ուղարկում
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveAccountClosingOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.AccountClosingOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveAccountClosingOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                AccountClosingOrder order = _cacheHelper.GetApprovalOrder<AccountClosingOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveAccountClosingOrder(order);

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
        /// Վերադարձնում է ընթացիկ հաշվի բացման հայտը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAccountOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetAccountOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xbService.GetAccountOrder(request.Id);
                var response = new SingleResponse<AccountOrder>();
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
        /// Ընթացիկ հաշվի բացման հայտի ուղարկում
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveAccountOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.AccountOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveAccountOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                AccountOrder order = _cacheHelper.GetApprovalOrder<AccountOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveAccountOrder(order);

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
        /// Ընթացիկ հաշվի բացման հայտի պահպանում
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveAccountOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveAccountOrder(AccountOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveAccountOrder(request.Order);

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
        /// Վերադարձնում է ընթացիկ հաշվի քաղվածքը՝ ՀՏ940 ֆորմատով
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetSwiftMessage940Statement")]
        public IActionResult GetSwiftMessage940Statement(AccountNumberAndDateRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<string>() { ResultCode = ResultCodes.normal };
                byte[] tempArray = System.Text.Encoding.UTF8.GetBytes(_xbService.GetSwiftMessage940Statement(request.DateFrom, request.DateTo, request.AccountNumber));
                response.Result = Convert.ToBase64String(tempArray);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        ///Վերադարձնում է ընթացիկ հաշվի քաղվածքը՝ ՀՏ950 ֆորմատով
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetSwiftMessage950Statement")]
        public IActionResult GetSwiftMessage950Statement(AccountNumberAndDateRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<string>() { ResultCode = ResultCodes.normal };
                byte[] tempArray = System.Text.Encoding.UTF8.GetBytes(_xbService.GetSwiftMessage950Statement(request.DateFrom, request.DateTo, request.AccountNumber));
                response.Result = Convert.ToBase64String(tempArray);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        ///Վերադարձնում է հաշվին կցված 3-րդ անձանց ցանկը (հօգուտ 3-րդ անձի/համատեղ հաշիվների դեպքում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAccountJointCustomers")]
        [TypeFilter(typeof(ValidateAccountNumberFilter))]
        public IActionResult GetAccountJointCustomers(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<JointCustomer>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetAccountJointCustomers(request.AccountNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        ///Վերադարձնում է՝ արդյոք տվյալ հաշվեհամարը ընթացիկ հաշվեհամար է, թե ոչ։
        /// </summary>
        /// <returns></returns>
        [HttpPost("IsCurrentAccount")]
        [TypeFilter(typeof(ValidateAccountNumberFilter))]
        public IActionResult IsCurrentAccount(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<bool>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.IsCurrentAccount(request.AccountNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        ///Ստուգում է՝ արդյոք նշված հաշվեհամարին փոխանցում կատարելիս անհրաժեշտ է մուտքագրել ՀԾՀ, թե ոչ
        /// </summary>
        /// <returns></returns>
        [HttpPost("CheckAccountForPSN")]
        [TypeFilter(typeof(ValidateAccountNumberFilter))]
        public IActionResult CheckAccountForPSN(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<bool>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.CheckAccountForPSN(request.AccountNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ընթացիկ հաշվի պայմանների ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCurrentAccountTarriffs")]
        public IActionResult GetCurrentAccountTarriffs()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                var language = _cacheHelper.GetLanguage();
                string HTMLCode = string.Empty;
                if (language == 1)
                {
                    HTMLCode = @"<!DOCTYPE html>
                                    <html>
                                    <head>
                                        <meta charset=" + "utf-8" + @" />
                                         <title></title>
                                    </head>
                                    <body>
                                        <div> 
                                         <b>Ընթացիկ հաշվի սպասարկման միջնորդավճար</b>
                                            <p>300 ՀՀ դրամ ամսական – եթե ամսվա միջին օրական մնացորդը բոլոր ընթացիկ և ավանդային հաշիվների գծով փոքր  է 20,000  ՀՀ դրամից ռեզիդենտ հաճախորդների դեպքում կամ 50,000  ՀՀ դրամից` ոչ ռեզիդենտ հաճախորդների դեպքում:</p>
                                            <p>Անվճար - ամսվա միջին օրական մնացորդը բոլոր ընթացիկ և ավանդային հաշիվների գծով մեծ կամ հավասար է վերոնշյալ շեմին:</p>
                                            <p><b>Կանխիկացում</b></p>
                                            <ul>
                                                <li>Անվճար- Հաշվից կանխիկացման միջնորդավճար կանխիկ մուտքի դեպքում</li>
                                                <li><p>0.3% (նվազագույնը 200 AMD) -Հաշվից կանխիկացման միջնորդավճար անկանխիկ մուտքի դեպքում (ՀՀ դրամով հաշիվներ)</p></li>
                                                <li><p>0.5% (նվազագույնը 1000 ՀՀ դրամ)- Հաշվից կանխիկացման միջնորդավճար անկանխիկ մուտքի դեպքում (արտարժութային հաշիվներ)</p></li>
                                            </ul>
                                        </div>
                                        <br>
                                       <div>
                                        <p>Պայմաններին առավել մանրամասն ծանոթանալու համար կարող եք այցելել <a href=" + @"https://www.acba.am/hy/individuals/Manage-accounts/current-account" + @" target=" + @"_blank" + @">acba.am</a></p>
                                     </div>
                                    </body>
                                    </html>";
                }
                else
                {
                    HTMLCode = @"<!DOCTYPE html>
                                    <html>
                                    <head>
                                        <meta charset=" + "utf-8" + @" />
                                         <title></title>
                                    </head>
                                    <body>
                                        <div> 
                                         <b>Current account service fee</b>
                                            <p>300 AMD monthly- if the average daily balance for all current and deposit accounts is less than 20,000 AMD for resident customers or 50,000 AMD for non-resident customers.</p>
                                            <p>Free of charge- if the average daily balance for all current and deposit accounts is equal or more than the amount mentioned above.</p>
                                            <p><b>Cash withdrawal</b></p>
                                            <ul>
                                                <li>Free of charge - in case if account was replenished in cash method</li>
                                                <li><p>0.3% (minimum 200 AMD)- in case if account was replenished in non-cash method (AMD currency accounts)</p></li>
                                                <li><p>0.5% (minimum 1000 AMD)- )- in case if account was replenished in non-cash method (foreign currency accounts)</p></li>
                                            </ul>
                                        </div>
                                        <br>
                                       <div>
                                        <p>For more information you can visit <a href=" + @"https://www.acba.am/hy/individuals/Manage-accounts/current-account" + @" target=" + @"_blank" + @">acba.am</a></p>
                                     </div>
                                    </body>
                                    </html>";
                }

                response.Result = HTMLCode;
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վարկային գծի և ընթացիկ հաշիվների արժույթների ստացում
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="orderSubtype"></param>
        /// <param name="orderAccountType"></param>
        /// <returns></returns>
        [HttpPost("GetDepositAndCurrentAccountCurrencies")]
        public IActionResult GetDepositAndCurrentAccountCurrencies(DepositAndCurrentAccCurrenciesRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<string>> response = new SingleResponse<List<string>>();
                response.Result = _xbService.GetDepositAndCurrentAccountCurrencies(request.OrderType, request.OrderSubtype, request.OrderAccountType);
                response.Result.Reverse();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Օվերդրաֆտի հաշվին կցված ընթացիկ հաշվի ստացում
        /// </summary>
        /// <param name="loanFullNumber"></param>
        /// <returns></returns>
        [HttpPost("GetCreditLineConnectAccount")]
        public IActionResult GetCreditLineConnectAccount(LoanFullNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<Account> response = new SingleResponse<Account>();
                response.Result = _xbService.GetCreditLineConnectAccount(request.LoanFullNumber);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }



        /// <summary>
        /// Վերադարձնում է ընթացիկ հաշվի վավերապայմանները
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        [HttpPost("GetCurrentAccountRequisites")]
        [TypeFilter(typeof(ValidateAccountNumberFilter))]
        public IActionResult GetCurrentAccountRequisites(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                Account account = _xbService.GetAccount(request.AccountNumber);
                string contractName = "";
                if (account != null)
                {
                    if (account.Currency == "AMD")
                    {
                        contractName = "BankMailTransferDetails";

                    }
                    else if (account.Currency == "RUR")
                    {
                        contractName = "SwiftTransferDetailsRUR";
                    }
                    else
                    {
                        contractName = "SwiftTransferDetails";
                    }
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add(key: "accountNumber", value: request.AccountNumber);


                    response.Result = _contractService.RenderContract(contractName, parameters, "CurrentAccountRequisites");
                    response.ResultCode = ResultCodes.normal;
                }
                else
                {
                    response.ResultCode = ResultCodes.failed;
                    response.Description = "Please, provide valid account number!";
                }

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է բացված ընթացիկ հաշվի պայմանագիրը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetOpenedAccountContract")]
        [TypeFilter(typeof(ValidateAccountNumberFilter))]
        public IActionResult GetOpenedAccountContract(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<byte[]>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetOpenedAccountContract(request.AccountNumber);
                if(response.Result == null)
                {
                    response.ResultCode = ResultCodes.validationError;
                    response.Description = "Կցված պայմանագիր առկա չէ";
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// վերադարձնում է փակված հաշիվները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetClosedAccounts")]
        public IActionResult GetClosedAccounts()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Account>> response = new SingleResponse<List<Account>>();
                response.Result = _xbService.GetClosedAccounts();

                Parallel.ForEach(response.Result, x => {
                    x.ProductNote = _xbService.GetProductNote(Convert.ToDouble(x.AccountNumber));
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
        /// Վերադարձնում է Հաշվի վերաբացման հայտը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetAccountReOpenOrder")]
        public IActionResult GetAccountReOpenOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<AccountReOpenOrder> response = new SingleResponse<AccountReOpenOrder>();
                response.Result = _xbService.GetAccountReOpenOrder(request.Id);
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
        /// Պահպանում է հաշվի վերաբացման հայտը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SaveAccountReOpenOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveAccountReOpenOrder(AccountReOpenOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                ActionResult saveResult = _xbService.SaveAccountReOpenOrder(request.Order);

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
        /// Հաստատում է հաշվի վերաբացման հայտը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("ApproveAccountReOpenOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.AccountReOpenOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]

        public IActionResult ApproveAccountReOpenOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };

                AccountReOpenOrder order = _cacheHelper.GetApprovalOrder<AccountReOpenOrder>(request.Id);

                ActionResult saveResult = _xbService.ApproveAccountReOpenOrder(order);

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
        /// Վերադարձնում է վարկի մարման հայտի դեպքում հասանելի ելքագրվող հաշիվների ցանկը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetAccountsForCurrency")]
        public IActionResult GetAccountsForCurrency(CurrencyRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<Account>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetAccountsForCurrency(request.Currency);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        ///Վերադարձնում է՝ արդյոք տվյալ հաշվեհամարը POS հաշվեհամար է, թե ոչ։
        /// </summary>
        /// <returns></returns>
        [HttpPost("IsPOSAccount")]
        [TypeFilter(typeof(ValidateAccountNumberFilter))]
        public IActionResult IsPOSAccount(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<bool>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.IsPOSAccount(request.AccountNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է բյուջե հաշվեհամարի մասին տեղեկատվություն
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetSearchedBudgetAccount")]
        public IActionResult GetSearchedBudgetAccount(SearchBudgetAccountRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<SearchBudgetAccount> response = new SingleResponse<SearchBudgetAccount>();
                response.ResultCode = ResultCodes.normal;
                request.SearchAccount.BeginRow = 1;
                request.SearchAccount.EndRow = 1; //Front Office-ում 20 է, քանի որ կարող են վերադարձվել 1-ից ավել տողեր, 
                                                  //որովեհտև այնտեղ որոնումը կատարվում է ոչ միայն հաշվեհամարով
                List<SearchBudgetAccount> accounts = _xbService.GetSearchedBudgetAccount(request.SearchAccount);
                response.Result = accounts.Count > 0 ? accounts[0] : null;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ընթացիկ, միավորված ընթացիկ և սնանկ հաշիվները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAccountsDigitalBanking")]
        public IActionResult GetAccountsDigitalBanking()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Account>> response = new SingleResponse<List<Account>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xbService.GetAccountsDigitalBanking();
                Parallel.ForEach(response.Result, x => {
                    //response.Result.RemoveAll(m => !HasProductPermission(m.AccountNumber));
                    x.HasContractFile = _xbService.HasUploadedContract(x.AccountNumber, 3);
                    x.ProductNote = _xbService.GetProductNote(Convert.ToDouble(x.AccountNumber));
                });
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

    }
}