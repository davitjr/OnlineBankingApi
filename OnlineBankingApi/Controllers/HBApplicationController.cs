using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary;
using OnlineBankingLibrary.Models;
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
    public class HBApplicationController : ControllerBase
    {
        private readonly XBService _xBService;
        private readonly XBInfoService _xBInfoService;
        private readonly CacheHelper _cacheHelper;
        private readonly XBSecurityService _xbSecurity;
        private readonly SMSMessagingService _smsMessagingService;
        private readonly CacheManager _cache;
        private readonly IConfiguration _config;
        public HBApplicationController(XBService xBService, XBInfoService xBInfoService, CacheHelper cacheHelper, XBSecurityService xBSecurity, SMSMessagingService sMSMessagingService, CacheManager cache, IConfiguration config)
        {
            _xBService = xBService;
            _xBInfoService = xBInfoService;
            _cacheHelper = cacheHelper;
            _xbSecurity = xBSecurity;
            _smsMessagingService = sMSMessagingService;
            _cache = cache;
            _config = config;
        }

        /// <summary>
        /// Պահպանում է տոկենի ապաբլոկավորման հայտը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SaveHBServletTokenUnBlockRequestOrder")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveHBServletTokenUnBlockRequestOrder(HBServletRequestOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xBService.SaveHBServletTokenUnBlockRequestOrder(request.Order);
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
        /// Հաստատում/ուղարկում է տոկենի ապաբլոկավորման հայտը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("ApproveHBServletTokenUnBlockRequestOrder")]
        [TypeFilter(typeof(CheckSignFilter), Arguments = new object[] { ApprovalOrderType.HBServletRequestOrder })]
        [TypeFilter(typeof(QualityChangingAbilityFilter))]
        [TypeFilter(typeof(SeconfConfirmationFilter))]
        public IActionResult ApproveHBServletTokenUnBlockRequestOrder(ApproveIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                HBServletRequestOrder order = _cacheHelper.GetApprovalOrder<HBServletRequestOrder>(request.Id);
                XBS.ActionResult result = _xBService.ApproveHBServletTokenUnBlockRequestOrder(order);
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
        /// Տոկենի ապաբլոկավորման կամ ակտիվացման հայտի Get
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetHBServletRequestOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetHBServletRequestOrder(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<HBServletRequestOrder> response = new SingleResponse<HBServletRequestOrder>
                {
                    Result = _xBService.GetHBServletRequestOrder(request.Id),
                    ResultCode = ResultCodes.normal
                };
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        /// <summary>
        /// Ստանալ տվյալ հաճախորդի բոլոր տոկենները
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetCustomerActiveTokens")]
        [AllowAnonymous]
        [TypeFilter(typeof(ValidateOtpFilter))]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> GetCustomerActiveTokens([FromBody] TokenOperationRequest request)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            SingleResponse<CustomerTokenResponse> response = new SingleResponse<CustomerTokenResponse>()
            {
                ResultCode = ResultCodes.normal
            };
            byte language = 1;
            if (!string.IsNullOrEmpty(Request.Headers["language"]))
                byte.TryParse(Request.Headers["language"], out language);
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            List<HBToken> hBTokens = new List<HBToken>();
            HBUser hBUser = new HBUser();
            Task<List<HBToken>> hBTokensTask = null;
            Task<HBUser> hBUserTask = null;

            _xBService.Use(client =>
            {
                hBTokensTask = client.GetFilteredHBTokensAsync(authorizedCustomer.UserId, HBTokenQuality.Active);
            });
            _xBService.Use(client =>
            {
                hBUserTask = client.GetHBUserAsync(authorizedCustomer.UserId);
            });
            hBUser = await hBUserTask;
            hBTokens = await hBTokensTask;
            if ((hBTokens?.Count ?? 0) == 0)
            {

                response.ResultCode = ResultCodes.validationError;
                response.Description = (Languages)language == Languages.hy ? "Փոխարինման հայտը հնարավոր չէ ուղարկել: Խնդրում ենք դիմել Բանկ:" : "Can't send replacement request. Please contact the Bank.";
                return ResponseExtensions.ToHttpResponse(response);
            }
            response.Result = new CustomerTokenResponse
            {
                IsNewHbUser = hBUser.IsCas,
                Tokens = new List<CustomerToken>()
            };
            foreach (var token in hBTokens)
            {
                response.Result.Tokens.Add(new CustomerToken
                {
                    TokenSerial = token.TokenNumber,
                    DeviceTypeDescription = token.DeviceTypeDescription
                });
            }
            return ResponseExtensions.ToHttpResponse(response);
        }
        /// <summary>
        /// "Փոխարինող" ենթատեսակով նոր տոկենի մուտքագրման հայտի պահպանում, ուղարկում \\ Ծառայությունների ակտիվացում հայտի պահպանում, ուղարկում\\Տոկենի ակտիվացման հայտ 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SaveAndApproveTokenReplacementOrder")]
        [AllowAnonymous]
        [TypeFilter(typeof(ValidateOtpFilter))]
        public async Task<IActionResult> SaveAndApproveTokenReplacementOrder([FromBody] TokenOperationRequest request)
        {
            if (ModelState.IsValid)
            {
                double oldDayLimit = 400000;
                double oldTransLimit = 400000;
                if (!request.IsNewHbUser)
                {
                    SingleResponse<long> response = new SingleResponse<long>();
                    byte language = 1;
                    if (!string.IsNullOrEmpty(Request.Headers["language"]))
                        byte.TryParse(Request.Headers["language"], out language);
                    AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                    ulong id = _xBInfoService.GetLastKeyNumber(22000, 74);
                    List<HBToken> activehBTokens = new List<HBToken>();
                    HBApplicationOrder order = new HBApplicationOrder()
                    {
                        Source = SourceType.MobileBanking,
                        HBApplication = new HBApplication(),
                        HBApplicationUpdate = new HBApplicationUpdate
                        {
                            AddedItems = new List<object>(),
                            DeactivatedItems = new List<object>(),
                            UpdatedItems = new List<object>()
                        }
                    };
                    _xBService.Use(client =>
                    {
                        activehBTokens = client.GetFilteredHBTokensAsync(authorizedCustomer.UserId, HBTokenQuality.Active).Result;
                    });
                    oldDayLimit = (activehBTokens.OrderByDescending(x => x.ActivationDate).FirstOrDefault()?.DayLimit ?? 0) != 0 ? activehBTokens.OrderByDescending(x => x.ActivationDate).FirstOrDefault().DayLimit : 400000;
                    oldTransLimit = (activehBTokens.OrderByDescending(x => x.ActivationDate).FirstOrDefault()?.TransLimit ?? 0) != 0 ? activehBTokens.OrderByDescending(x => x.ActivationDate).FirstOrDefault().TransLimit : 400000;
                    ActionResult result = _xBService.SaveAndApproveHBApplicationNewOrder(order, out HBToken hBToken, id, authorizedCustomer.UserId, oldDayLimit, oldTransLimit);
                    if (result.ResultCode == (ResultCode)ResultCodes.normal)
                    {
                        hBToken.HBUser.IsCas = true;
                        HBServletRequestOrder hBServletRequestOrder = new HBServletRequestOrder
                        {
                            RegistrationDate = DateTime.Now,
                            OperationDate = _xBService.GetCurrentOperDay(),
                            ServletAction = HBServletAction.ActivateToken,
                            Type = OrderType.HBServletRequestTokenActivationOrder,
                            SubType = 1,
                            CustomerNumber = authorizedCustomer.CustomerNumber,
                            ServletRequest = null,
                            Source = SourceType.MobileBanking,
                            PhoneNumber = GetCustomerRegPhone(authorizedCustomer.CustomerNumber),
                            HBtoken = new HBToken
                            {
                                TokenNumber = hBToken.TokenNumber,
                                DayLimit = oldDayLimit,
                                TransLimit = oldTransLimit,
                                ID = hBToken.ID,
                                GID = hBToken.GID,
                                TokenType = hBToken.TokenType,
                                HBUser = hBToken.HBUser,
                                IsRegistered = true
                            },
                            FilialCode = 22000
                        };
                        ActionResult saveActionResult = _xBService.SaveAndApproveTokenOrder(hBServletRequestOrder);
                        if (saveActionResult.ResultCode == ResultCode.DoneAndReturnedValues || saveActionResult.ResultCode == ResultCode.Normal) //եթե հին մոբայլի user -  ը (iscas == false)  ստանում է նոր տոկեն աֆտոմատ սարքում ենք նոր մոբայլի user 
                        {
                            _xBService.MigrateOldUserToCas(hBToken.HBUser.ID);
                            response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(saveActionResult.ResultCode);
                            response.Result = saveActionResult.Id;
                            response.Description = (Languages)language == Languages.hy ? "Ակտիվացման կոդը ուղարկվել է Ձեր էլ. փոստին, իսկ PIN կոդը կստանանք SMS-ի տեսքով:" : "The activation code was sent to your email. PIN code was sent by SMS.";
                        }
                        else
                        {
                            response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(saveActionResult.ResultCode);
                            response.Result = saveActionResult.Id;
                            response.Description = saveActionResult.Errors[0].Code == 0 ? saveActionResult.Errors[0].Description : _xBService.GetTerm(saveActionResult.Errors[0].Code, null, (Languages)language);
                        }
                    }
                    else
                    {
                        response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                        response.Result = result.Id;
                        response.Description = _xBService.GetTerm(result.Errors[0].Code, null, Languages.hy);
                    }
                    return ResponseExtensions.ToHttpResponse(response);
                }
                else
                {
                    SingleResponse<long> response = new SingleResponse<long>();
                    ActionResult result = new ActionResult();
                    List<HBActivationRequest> hBActivation = new List<HBActivationRequest>();
                    HBApplicationOrder order = new HBApplicationOrder()
                    {
                        Source = SourceType.MobileBanking
                    };
                    HBActivationOrder hBActivationOrder = new HBActivationOrder();
                    List<HBToken> hBTokens = new List<HBToken>();
                    HBToken hBToken = new HBToken();
                    HBApplication hBApplication = new HBApplication();
                    Task<List<HBToken>> hBTokensTask = null;
                    Task<HBToken> hBTokenTask = null;
                    Task<HBApplication> hBApplicationTask = null;
                    ulong id = _xBInfoService.GetLastKeyNumber(22000, 74);
                    AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                    byte language = 1;
                    if (!string.IsNullOrEmpty(Request.Headers["language"]))
                        byte.TryParse(Request.Headers["language"], out language);

                    _xBService.Use(client =>
                    {
                        hBTokensTask = client.GetFilteredHBTokensAsync(authorizedCustomer.UserId, HBTokenQuality.NotDefined);
                    });
                    _xBService.Use(client =>
                    {
                        hBTokenTask = client.GetHBTokenWithSerialNumberAsync(request.TokenSerial);
                    });
                    _xBService.Use(client =>
                    {
                        hBApplicationTask = client.GetHBApplicationAsync();
                    });
                    hBTokens = await hBTokensTask;
                    hBApplication = await hBApplicationTask;
                    hBToken = await hBTokenTask;
                    oldDayLimit = (hBToken?.DayLimit ?? 0) != 0 ? hBToken.DayLimit : 400000;
                    oldTransLimit = (hBToken?.TransLimit ?? 0) != 0 ? hBToken.TransLimit : 400000;
                    if (authorizedCustomer.TypeOfClient != 6 && hBTokens.All(x => x.TokenType == HBTokenTypes.Token) && hBApplication?.ContractDate < new DateTime(2015, 7, 25)) // Ն-19051
                    {
                        response.ResultCode = ResultCodes.validationError;
                        response.Description = (Languages)language == Languages.hy ? "Փոխարինման հայտը հնարավոր չէ ուղարկել: Խնդրում ենք դիմել Բանկ:" : "Can't send replacement request. Please contact the Bank.";
                        return ResponseExtensions.ToHttpResponse(response);
                    }
                    if (hBToken == null)
                    {

                        result.ResultCode = ResultCode.ValidationError;
                        response.Description = (Languages)language == Languages.hy ? "Տվյալ համարով տոկեն գոյություն չունի։" : "Token not exist.";
                        return ResponseExtensions.ToHttpResponse(response);
                    }
                    HBServletRequestOrder hBServletRequestOrder = new HBServletRequestOrder
                    {
                        RegistrationDate = DateTime.Now,
                        OperationDate = _xBService.GetCurrentOperDay(),
                        ServletAction = HBServletAction.DeactivateToken,
                        Type = XBS.OrderType.HBServletRequestTokenDeactivationOrder,
                        SubType = 1,
                        CustomerNumber = authorizedCustomer.CustomerNumber,
                        ServletRequest = null,
                        Source = SourceType.MobileBanking,
                        HBtoken = hBToken,
                        FilialCode = 22000


                    };
                    ActionResult saveActionResult = _xBService.SaveAndApproveTokenOrder(hBServletRequestOrder);
                    if (saveActionResult.ResultCode == (ResultCode)ResultCodes.normal)
                    {
                        result = _xBService.SaveAndApproveHBApplicationReplacmentOrder(order, ref hBToken, id, authorizedCustomer.UserId, request.TokenSerial, oldDayLimit, oldTransLimit);
                        if (result.ResultCode == (ResultCode)ResultCodes.normal)
                        {
                            hBServletRequestOrder.RegistrationDate = DateTime.Now;
                            hBServletRequestOrder.OperationDate = _xBService.GetCurrentOperDay();
                            hBServletRequestOrder.ServletAction = HBServletAction.ActivateToken;
                            hBServletRequestOrder.Type = OrderType.HBServletRequestTokenActivationOrder;
                            hBServletRequestOrder.SubType = 1;
                            hBServletRequestOrder.Source = SourceType.MobileBanking;
                            hBServletRequestOrder.CustomerNumber = authorizedCustomer.CustomerNumber;
                            hBServletRequestOrder.PhoneNumber = GetCustomerRegPhone(authorizedCustomer.CustomerNumber);
                            hBServletRequestOrder.ServletRequest = null;
                            hBServletRequestOrder.HBtoken = new HBToken
                            {
                                TokenNumber = hBToken.TokenNumber,
                                DayLimit = oldDayLimit,
                                TransLimit = oldTransLimit,
                                ID = hBToken.ID,
                                GID = hBToken.GID,
                                TokenType = hBToken.TokenType,
                                HBUser = hBToken.HBUser,
                                IsRegistered = true
                            };
                            hBServletRequestOrder.FilialCode = 22000;

                            ActionResult saveTokenResult = _xBService.SaveAndApproveTokenOrder(hBServletRequestOrder);
                            if ((saveTokenResult.ResultCode == ResultCode.DoneAndReturnedValues || saveTokenResult.ResultCode == ResultCode.Normal)) //եթե հին մոբայլի user -  ը (iscas == false)  ստանում է նոր տոկեն աֆտոմատ սարքում ենք նոր մոբայլի user 
                            {
                                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(saveTokenResult.ResultCode);
                                response.Result = saveTokenResult.Id;
                                response.Description = (Languages)language == Languages.hy ? "Ակտիվացման կոդը ուղարկվել է Ձեր էլ. փոստին, իսկ PIN կոդը կստանանք SMS-ի տեսքով:" : "The activation code was sent to your email. PIN code was sent by SMS.";
                            }
                            else
                            {
                                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                                response.Result = result.Id;
                                response.Description = _xBService.GetTerm(saveTokenResult.Errors[0].Code, null, (Languages)language);
                            }
                        }
                        else
                        {
                            response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                            response.Result = result.Id;
                            response.Description = _xBService.GetTerm(result.Errors[0].Code, null, (Languages)language);
                        }
                    }
                    else
                    {
                        response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(saveActionResult.ResultCode);
                        response.Result = saveActionResult.Id;
                        response.Description = _xBService.GetTerm(saveActionResult.Errors[0].Code, null, (Languages)language);
                    }
                    return ResponseExtensions.ToHttpResponse(response);
                }
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        /// <summary>
        /// տոկենի դեակտիվացման հայտի պահպանում, ուղարկում , տոկենի դեակտիվացում
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SaveAndApproveTokenDeactivationOrder")]
        [TypeFilter(typeof(ValidateOtpFilter))]
        [AllowAnonymous]
        public IActionResult SaveAndApproveTokenDeactivationOrder([FromBody] TokenOperationRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                HBToken hBToken = new HBToken();
                ActionResult result = new ActionResult();
                AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                _xBService.Use(client =>
                {
                    hBToken = client.GetHBTokenWithSerialNumberAsync(request.TokenSerial).Result;
                });
                if (hBToken == null)
                {
                    byte language = 1;

                    //Լեզու
                    if (!string.IsNullOrEmpty(Request.Headers["language"]))
                        byte.TryParse(Request.Headers["language"], out language);

                    result.ResultCode = ResultCode.ValidationError;
                    response.Description = (Languages)language == Languages.hy ? "Տվյալ համարով տոկեն գոյություն չունի։" : "Token not exist.";
                }
                else
                {
                    //string sessionId = Guid.NewGuid().ToString();
                    //AuthorizeAnonymousMethods(hBToken.HBUser.CustomerNumber, sessionId, hBToken.HBUser.ID, hBToken.HBUser.UserName);
                    //Request.Headers.Add("SessionId", sessionId);

                    HBServletRequestOrder hBServletRequestOrder = new HBServletRequestOrder
                    {
                        RegistrationDate = DateTime.Now,
                        OperationDate = _xBService.GetCurrentOperDay(),
                        ServletAction = HBServletAction.DeactivateToken,
                        Type = OrderType.HBServletRequestTokenDeactivationOrder,
                        SubType = 1,
                        CustomerNumber = authorizedCustomer.CustomerNumber,
                        ServletRequest = null,
                        HBtoken = hBToken,
                        Source = SourceType.MobileBanking,
                        FilialCode = 22000
                    };

                    ActionResult saveActionResult = _xBService.SaveAndApproveTokenOrder(hBServletRequestOrder);
                    response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(saveActionResult.ResultCode);
                    response.Result = saveActionResult.Id;
                    response.Description = utils.GetActionResultErrors(saveActionResult.Errors);
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        /// <summary>
        /// User-ի նույնականցում եվ ապաբլոկավորում
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveAndApproveUserUnlockOrder")]
        [AllowAnonymous]
        public IActionResult SaveAndApproveUserUnlockOrder([FromBody] TokenOperationRequestWithAuthorization request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>()
                {
                    ResultCode = ResultCodes.normal
                };
                XBSecurity.LoginInfo loginInfo = new XBSecurity.LoginInfo()
                {
                    ForUnlocking = true
                };
                byte language = 1;
                //Լեզու
                if (!string.IsNullOrEmpty(Request.Headers["language"]))
                    byte.TryParse(Request.Headers["language"], out language);

                //Օգտագործող 
                if (!string.IsNullOrEmpty(request.UserName))
                {
                    loginInfo.UserName = request.UserName;
                }
                else
                {
                    response.ResultCode = ResultCodes.validationError;
                    response.Description = (Languages)language == Languages.hy ? "Մուտքագրեք ձեր օգտվողի անունը։" : "Please enter your username.";
                }
                //Գաղտնաբառ  
                if (!string.IsNullOrEmpty(request.Password))
                {
                    loginInfo.Password = utils.GetSHA1Hash(request.Password);
                }
                else
                {
                    response.ResultCode = ResultCodes.validationError;
                    response.Description = (Languages)language == Languages.hy ? "Մուտքագրեք ձեր գաղտնաբառը։" : "Please enter your password.";
                }

                if (response.ResultCode == ResultCodes.normal)
                {
                    loginInfo.IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();

                    var AuthResponce = _xbSecurity.AuthorizeUserByUserPassword(loginInfo, language);

                    if (AuthResponce.AuthorizationResult.IsAuthorized)
                    {
                        if (AuthResponce.AuthorizationResult.ResultCode == XBSecurity.LoginResultCode.ChangePassword)
                        {
                            response.ResultCode = ResultCodes.validationError;
                            response.Description = $"{(!string.IsNullOrEmpty(AuthResponce.AuthorizationResult.DescriptionAM) ? AuthResponce.AuthorizationResult.DescriptionAM + " " : "")}{AuthResponce.AuthorizationResult.Description}";
                            return ResponseExtensions.ToHttpResponse(response);
                        }
                        HBUser hbUser = new HBUser();
                        _xBService.Use(client =>
                        {
                            hbUser = client.GetHBUserByUserNameAsync(request.UserName).Result;
                        });
                        AuthorizeAnonymousMethods(hbUser.CustomerNumber, AuthResponce.AuthorizationResult.SessionID.ToString(), hbUser.ID, hbUser.UserName);
                        if (Request.Headers.ContainsKey("SessionId"))
                        {
                            Request.Headers.Remove("SessionId");
                        }
                        Request.Headers.Add("SessionId", AuthResponce.AuthorizationResult.SessionID.ToString());
                        HBServletRequestOrder hBServletRequestOrder = new HBServletRequestOrder
                        {
                            RegistrationDate = DateTime.Now,
                            OperationDate = _xBService.GetCurrentOperDay(),
                            ServletAction = HBServletAction.UnlockUser,
                            Type = OrderType.HBServletRequestTokenUnBlockOrder,
                            SubType = 1,
                            ServletRequest = new TokenOperationsInfo1(),
                            HBtoken = new HBToken
                            {
                                HBUser = new HBUser
                                {
                                    UserName = hbUser.UserName,
                                    IsCas = true
                                }
                            },
                            CustomerNumber = hbUser.CustomerNumber,
                            Source = SourceType.MobileBanking,
                            FilialCode = 22000,
                            PhoneNumber = GetCustomerRegPhone(hbUser.CustomerNumber)
                        };
                        ActionResult saveActionResult = _xBService.SaveAndApproveTokenOrder(hBServletRequestOrder);
                        response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(saveActionResult.ResultCode);
                        response.Result = saveActionResult.Id;
                        if (response.ResultCode == ResultCodes.normal)
                        {
                            response.Description = (Languages)language == Languages.hy ? "Օգտագործողը ապաբլոկավորված է:" : "User unblocked";
                        }
                        else
                        {
                            response.Description = utils.GetActionResultErrors(saveActionResult.Errors);
                        }
                        return ResponseExtensions.ToHttpResponse(response);
                    }
                    else
                    {
                        response.ResultCode = ResultCodes.notAuthorized;
                        response.Description = $"{(!string.IsNullOrEmpty(AuthResponce.AuthorizationResult.DescriptionAM) ? AuthResponce.AuthorizationResult.DescriptionAM + " " : "")}{AuthResponce.AuthorizationResult.Description}";
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
        /// նոր տոկենի մուտքագրման հայտի պահպանում, ուղարկում \\ Ծառայությունների ակտիվացում հայտի պահպանում, ուղարկում\\Տոկենի ակտիվացման հայտ 
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveAndApproveNewTokenOrder")]
        [AllowAnonymous]
        [TypeFilter(typeof(ValidateOtpFilter))]
#pragma warning disable IDE0060 // Remove unused parameter
        public IActionResult SaveAndApproveNewTokenOrder([FromBody] TokenOperationRequest request)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                double DayLimit = 400000;
                double TransLimit = 400000;
                AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                ulong id = _xBInfoService.GetLastKeyNumber(22000, 74);
                HBApplicationOrder order = new HBApplicationOrder()
                {
                    Source = SourceType.MobileBanking,
                    HBApplication = new HBApplication(),
                    HBApplicationUpdate = new HBApplicationUpdate
                    {
                        AddedItems = new List<object>(),
                        DeactivatedItems = new List<object>(),
                        UpdatedItems = new List<object>()
                    }
                };
                ActionResult result = _xBService.SaveAndApproveHBApplicationNewOrder(order, out HBToken hBToken, id, authorizedCustomer.UserId, DayLimit, TransLimit);
                if (result.ResultCode == (ResultCode)ResultCodes.normal)
                {
                    HBServletRequestOrder hBServletRequestOrder = new HBServletRequestOrder
                    {
                        RegistrationDate = DateTime.Now,
                        OperationDate = _xBService.GetCurrentOperDay(),
                        ServletAction = HBServletAction.ActivateToken,
                        Type = OrderType.HBServletRequestTokenActivationOrder,
                        SubType = 1,
                        CustomerNumber = authorizedCustomer.CustomerNumber,
                        ServletRequest = null,
                        Source = SourceType.MobileBanking,
                        PhoneNumber = GetCustomerRegPhone(authorizedCustomer.CustomerNumber),
                        HBtoken = new HBToken
                        {
                            TokenNumber = hBToken.TokenNumber,
                            DayLimit = 400000,
                            TransLimit = 400000,
                            ID = hBToken.ID,
                            GID = hBToken.GID,
                            TokenType = hBToken.TokenType,
                            HBUser = hBToken.HBUser,
                            IsRegistered = true
                        },
                        FilialCode = 22000
                    };

                    ActionResult saveActionResult = _xBService.SaveAndApproveTokenOrder(hBServletRequestOrder);
                    if ((saveActionResult.ResultCode == ResultCode.DoneAndReturnedValues || saveActionResult.ResultCode == ResultCode.Normal) && hBToken?.HBUser?.IsCas == false) //եթե հին մոբայլի user -  ը (iscas == false)  ստանում է նոր տոկեն աֆտոմատ սարքում ենք նոր մոբայլի user 
                    {
                        _xBService.MigrateOldUserToCas(hBToken.HBUser.ID);
                    }
                    response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(saveActionResult.ResultCode);
                    response.Result = saveActionResult.Id;
                    response.Description = utils.GetActionResultErrors(saveActionResult.Errors);
                }
                else
                {
                    response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                    response.Result = result.Id;
                    response.Description = result?.Errors.Count > 0 ? _xBService.GetTerm(result?.Errors[0]?.Code ?? 0, null, Languages.hy) : default;
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        /// <summary>
        /// User-ի նույնականցման համար OTP-ի գեներացում և sms-ով ուղարկում
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("VerifyUserAndSendOtpSms")]
        [AllowAnonymous]
        public IActionResult VerifyUserAndSendOtpSms([FromBody] VerifyUserAndSendOtpSmsRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>()
                {
                    ResultCode = ResultCodes.normal
                };
                XBSecurity.LoginInfo loginInfo = new XBSecurity.LoginInfo();
                byte language = 1;
                SourceType sourceType = SourceType.MobileBanking;
                //Լեզու
                if (!string.IsNullOrEmpty(Request.Headers["language"]))
                    byte.TryParse(Request.Headers["language"], out language);

                //Տվյալների մուտքագրման աղբյուր
                if (!string.IsNullOrEmpty(Request.Headers["SourceType"]))
                    Enum.TryParse(Request.Headers["SourceType"], out sourceType);

                //Օգտագործող 
                if (!string.IsNullOrEmpty(request.UserName))
                {
                    loginInfo.UserName = request.UserName;
                }
                else
                {
                    response.ResultCode = ResultCodes.validationError;
                    response.Description = (Languages)language == Languages.hy ? "Մուտքագրեք ձեր օգտվողի անունը։" : "Please enter your username.";
                }
                //Գաղտնաբառ  
                if (!string.IsNullOrEmpty(request.Password))
                {
                    loginInfo.Password = utils.GetSHA1Hash(request.Password);
                }
                else
                {
                    response.ResultCode = ResultCodes.validationError;
                    response.Description = (Languages)language == Languages.hy ? "Մուտքագրեք ձեր գաղտնաբառը։" : "Please enter your password.";
                }

                if (response.ResultCode == ResultCodes.normal)
                {
                    loginInfo.IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();

                    var AuthResponce = _xbSecurity.AuthorizeUserByUserPassword(loginInfo, language);

                    if (AuthResponce.AuthorizationResult.IsAuthorized)
                    {
                        HBUser hBUser = new HBUser();
                        _xBService.Use(client =>
                        {
                            hBUser = client.GetHBUserByUserNameAsync(request.UserName).Result;
                        });
                        // Get Customer Reg Phone
                        string regPhone = GetCustomerRegPhone(hBUser.CustomerNumber);
                        // Generate and Send Otp with sms
                        string otp = SendVerificationCode(regPhone, hBUser.ID, 5, CustomerRegistrationVerificationSMSTypes.NumbersAndLetters);
                        string guid = Guid.NewGuid().ToString();

                        CustomerTokenInfo customerTokenInfo = new CustomerTokenInfo()
                        {
                            CustomerNumber = hBUser.CustomerNumber,
                            SessionId = guid,
                            Otp = otp,
                            PhoneNumber = regPhone,
                            Email = hBUser?.Email?.email?.emailAddress,
                            UserId = hBUser.ID,
                            UserName = hBUser.UserName,
                            SourceType = sourceType,
                            Language = language,
                            Checked = false
                        };
                        // Save Customer Token Info in cache with guid
                        _cacheHelper.SetCustomerTokenInfo(customerTokenInfo);

                        response.Result = guid;
                    }
                    else
                    {
                        response.ResultCode = ResultCodes.notAuthorized;
                        response.Description = $"{(!string.IsNullOrEmpty(AuthResponce.AuthorizationResult.DescriptionAM) ? AuthResponce.AuthorizationResult.DescriptionAM + " " : "")}{AuthResponce.AuthorizationResult.Description}";
                    }
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        private string GetCustomerRegPhone(ulong customerNumber)
        {
            List<CustomerPhone> PhoneList = _xBService.GetPhoneNumbers(customerNumber);
            //Վերցնում է միայն բջջային հեռախոսահամարը
            PhoneList.RemoveAll(phone => phone.phoneType.key != 1 || phone.phone.countryCode != "+374");
            CustomerPhone customerPhone = PhoneList.LastOrDefault();
            string phoneNumber = customerPhone?.phone?.countryCode + customerPhone?.phone?.areaCode + customerPhone?.phone?.phoneNumber;

            return phoneNumber;
        }
        private string SendVerificationCode(string phoneNumber, int userId, byte verificationCodeLength, CustomerRegistrationVerificationSMSTypes type)
        {
            Random rnd = new Random();
            string verificationCode;
            string chars = type switch
            {
                CustomerRegistrationVerificationSMSTypes.OnlyNumbers => "0123456789",
                CustomerRegistrationVerificationSMSTypes.OnlyLetters => "abcdefghijklmnopqrstuvwxyz",
                CustomerRegistrationVerificationSMSTypes.NumbersAndLetters => "abcdefghijklmnopqrstuvwxyz01234567890123456789",
                _ => throw new Exception("Message type is not selected"),
            };
            if (!Convert.ToBoolean(_config["TestVersion"]))
            {
                verificationCode = new string(Enumerable.Repeat(chars, verificationCodeLength).Select(s => s[rnd.Next(s.Length)]).ToArray());
                _smsMessagingService.SendMessage(phoneNumber, 0, $"Mekangamya stugich code ` {verificationCode}", userId, 19);
            }
            else
            {
                verificationCode = "12345";
            }

            return verificationCode;
        }
        /// <summary>
        /// Only for some token functions with Allow Anonymous attribute and without ValidateOtpFilter filter (Seting header values and authorize customer in cache)
        /// </summary>
        private void AuthorizeAnonymousMethods(ulong CustomerNumber, string sessionId, int userId, string userName)
        {
            byte language = 1;
            SourceType sourceType = SourceType.MobileBanking;
            //Լեզու
            if (!string.IsNullOrEmpty(Request.Headers["language"]))
                byte.TryParse(Request.Headers["language"], out language);

            //Տվյալների մուտքագրման աղբյուր
            if (!string.IsNullOrEmpty(Request.Headers["SourceType"]))
                Enum.TryParse(Request.Headers["SourceType"], out sourceType);

            AuthorizedCustomer authorizedCustomer = new AuthorizedCustomer()
            {
                CustomerNumber = CustomerNumber,
                UserId = userId,
                SessionID = sessionId,
                UserName = userName
            };
            _cache.Set(sessionId + "_authorizedCustomer", authorizedCustomer);
            _cache.Set(sessionId + "_Language", language);
            _cache.Set(sessionId + "_SourceType", sourceType);
            _cache.Set(sessionId + "_ClientIp", Request.HttpContext.Connection.RemoteIpAddress.ToString());
        }
    }
}