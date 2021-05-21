using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Resources;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Models;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using XBSecurity;
using static OnlineBankingApi.Enumerations;
using static OnlineBankingApi.Utilities.LoggerUtility;
using ResponseExtensions = OnlineBankingApi.Models.ResponseExtensions;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly XBSecurityService _xbSecurity;
        private readonly XBService _xbService;
        private readonly CacheManager _cache;
        private readonly IStringLocalizer _localizer;

        public AuthorizationController(XBSecurityService xbSecurity, XBService xbService, IConfiguration config, CacheManager cache, IStringLocalizer<SharedResource> localizer)
        {
            _xbSecurity = xbSecurity;
            _xbService = xbService;
            _config = config;
            _cache = cache;
            _localizer = localizer;
        }


        /// <summary>
        /// Ստուգում է OTP-ին
        /// </summary>
        /// <returns></returns>
        [HttpPost("ValidateOTP")]
        public IActionResult ValidateOTP(OTPRequest request)
        {
            if (ModelState.IsValid)
            {
                string sessionId = "";
                byte language = 0;
                string ipAddress = "";
                bool isValid = false;

                short sourceType = 0;

                var response = new SingleResponse<bool>() { ResultCode = ResultCodes.normal };
                var context = Request.HttpContext;

                //Սեսիայի նունականացման համար
                if (!string.IsNullOrEmpty(context.Request.Headers["SessionId"]))
                    sessionId = context.Request.Headers["SessionId"];

                //IP հասցե
                ipAddress = context.Connection.RemoteIpAddress.ToString();

                //Լեզու
                if (!string.IsNullOrEmpty(context.Request.Headers["language"]))
                    byte.TryParse(context.Request.Headers["language"], out language);


                if (!string.IsNullOrEmpty(context.Request.Headers["SourceType"]))
                    short.TryParse(context.Request.Headers["SourceType"], out sourceType);

                if ((XBS.SourceType)sourceType == XBS.SourceType.MobileBanking)
                {
                    isValid = _xbSecurity.ValidateOTP(sessionId, request.OTP, ipAddress, language);
                }
                //else
                //{
                //    isValid = _xbSecurity.VerifyToken(sessionId, OTP, ipAddress, language);
                //}


                //Եթե անցել է նույնականացում
                if (isValid)
                {
                    response.ResultCode = ResultCodes.normal;
                    response.Result = true;
                }
                else
                {
                    response.ResultCode = ResultCodes.validationError;
                    response.Result = false;
                    response.Description = _localizer["Սխալ PIN կոդ։"];// "Incorrect PIN code."; 
                }

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ստուգում է գրանցման կոդը
        /// </summary>
        /// <param name="deviceTypeDescription">Սարքավորման նկարագրություն</param>
        /// <returns></returns>
        [HttpPost("ValidateRegistrationCode")]
        public IActionResult ValidateRegistrationCode([FromHeader][Required] string deviceTypeDescription, ValidateRegistrationCode request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<bool>() { ResultCode = ResultCodes.normal };
                byte language = 0;
                //Լեզու
                if (!string.IsNullOrEmpty(Request.HttpContext.Request.Headers["language"]))
                    byte.TryParse(Request.HttpContext.Request.Headers["language"], out language);
                response.Result = _xbSecurity.CheckRegistrationCode(request.RegistrationCode, deviceTypeDescription, request.UserName);
                if (response.Result != true)
                {
                    response.ResultCode = ResultCodes.validationError;
                    response.Description = _localizer["Մուտքագրված տվյալներ սխալ են կամ ոչ լիարժեք: Խնդրում ենք ստուգել օգտագործողի անունը և ակտիվացման կոդը։"];// "Wrong data. Please check the username and the activation code.";
                }
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Փոփոխում է օգտագործողի գաղտնաբառը
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChangeUserPassword")]
        [LoggerOff]
        [TypeFilter(typeof(AuthorizationFilter))]
        public IActionResult ChangeUserPassword(ChangeUserPasswordRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new Response() { ResultCode = ResultCodes.normal };
                var context = Request.HttpContext;

                ChangePasswordInfo changePasswordInfo = new ChangePasswordInfo();
                changePasswordInfo.Password = Utils.GetSHA1Hash(request.Password);
                changePasswordInfo.NewPassword = Utils.GetSHA1Hash((request.NewPassword));
                changePasswordInfo.RetypePassword = Utils.GetSHA1Hash((request.RetypeNewPassword));

                string sessionId = "";
                byte language = 0;

                //Սեսիայի նունականացման համար
                if (!string.IsNullOrEmpty(context.Request.Headers["SessionId"]))
                    sessionId = context.Request.Headers["SessionId"];

                //IP հասցե
                changePasswordInfo.IpAddress = context.Connection.RemoteIpAddress.ToString();

                //Լեզու
                if (!string.IsNullOrEmpty(context.Request.Headers["language"]))
                    byte.TryParse(context.Request.Headers["language"], out language);

                var changePasswordResult = _xbSecurity.ChangeUserPassword(changePasswordInfo, sessionId, language);

                //Եթե անցել է նույնականացում
                if (changePasswordResult.AuthorizationResult.IsAuthorized)
                {
                    if (changePasswordResult.PasswordChangeResult.IsChanged)
                    {
                        response.ResultCode = ResultCodes.normal;
                    }
                    else
                    {
                        response.ResultCode = ResultCodes.failed;
                        response.Description = changePasswordResult.PasswordChangeResult.Description;
                    }
                }
                else
                {
                    response.ResultCode = ResultCodes.failed;
                    response.Description = changePasswordResult.AuthorizationResult.Description;
                }

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Իրականացնում է գաղտնաբառի վերականգնում
        /// </summary>
        /// <returns></returns>
        [HttpPost("ResetUserPassword")]
        public IActionResult ResetUserPassword(PasswordResetRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<bool> response = new SingleResponse<bool>();

                var loginInfo = new LoginInfo();

                loginInfo.UserName = request.UserName;

                loginInfo.OTP = request.OTP;

                loginInfo.IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();

                response.Result = _xbSecurity.ResetUserPassword(loginInfo);

                response.ResultCode = ResultCodes.normal;

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        [HttpPost("AuthorizeByPassword")]
        //[LoggerOff]
        public IActionResult AuthorizeByPassword(AuthorizeByPasswordRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();

                byte language = 1;
                var loginInfo = new LoginInfo();

                //Օգտագործող 
                if (!string.IsNullOrEmpty(request.UserName))
                    loginInfo.UserName = request.UserName;


                //Գաղտնաբառ  
                if (!string.IsNullOrEmpty(request.Password))
                    loginInfo.Password = Utils.GetSHA1Hash(request.Password);

                XBS.SourceType sourceType = XBS.SourceType.NotSpecified;
                if (!string.IsNullOrEmpty(Request.Headers["SourceType"]))
                    Enum.TryParse(Request.Headers["SourceType"], out sourceType);

                if (!string.IsNullOrEmpty(Request.Headers["Language"]))
                    byte.TryParse(Request.Headers["Language"], out language);

                if (sourceType == XBS.SourceType.AcbaOnline)
                    loginInfo.PlatformType = PlatformType.OnlineBanking;
                else if (sourceType == XBS.SourceType.MobileBanking)
                    loginInfo.PlatformType = PlatformType.MobileBanking;
                loginInfo.IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();

                loginInfo.ForUnlocking = request.ForUnlocking;

                //if (!string.IsNullOrEmpty(Request.HttpContext.Request.Headers["Dev"]))
                //{
                //    loginInfo.AdditionalDetails.Add("OS", Request.HttpContext.Request.Headers["Dev"]);
                //}
                //else
                //{
                //    loginInfo.AdditionalDetails.Add("OS", "0");
                //}

                var aoUserData = _xbSecurity.AuthorizeUserByUserPassword(loginInfo, language, request.HostName);


                if (aoUserData.AuthorizationResult.IsAuthorized)
                {
                    string sessionId = Guid.NewGuid().ToString();
                    _cache.Set(sessionId + "_UserName", loginInfo.UserName);
                    _cache.Set(sessionId + "_LoginResult", aoUserData.AuthorizationResult);

                    if (!string.IsNullOrEmpty(Request.Headers["SourceType"]))
                        Enum.TryParse(Request.Headers["SourceType"], out sourceType);
                    _cache.Set(sessionId + "_SourceType", sourceType);
                    response.ResultCode = ResultCodes.normal;
                    response.Result = sessionId;
                }
                else
                {
                    response.ResultCode = ResultCodes.validationError;
                    response.Description = aoUserData.AuthorizationResult.DescriptionAM + " " + aoUserData.AuthorizationResult.Description;
                }




                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        [HttpPost("AuthorizeByToken")]
        //[LoggerOff]
        public IActionResult AuthorizeByToken(AuthorizeByTokenRequest request, [FromHeader]string Dev)
        {
            var response = new SingleResponse<AuthorizationResponse>();
            var result = new AuthorizationResponse();
            var loginInfo = new LoginInfo();
            string sessionId = "";
            byte lang = 1;

            if (!string.IsNullOrEmpty(Request.Headers["SessionId"]))
                sessionId = Request.Headers["SessionId"];

            loginInfo.UserName = _cache.Get<string, string>(sessionId + "_UserName");
            loginInfo.IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            XBS.SourceType sourceType = XBS.SourceType.NotSpecified;

            if (!string.IsNullOrEmpty(Request.Headers["SourceType"]))
                Enum.TryParse(Request.Headers["SourceType"], out sourceType);

            if (!string.IsNullOrEmpty(Request.Headers["Language"]))
                byte.TryParse(Request.Headers["Language"], out lang);

            if (sourceType == XBS.SourceType.AcbaOnline)
                loginInfo.PlatformType = PlatformType.OnlineBanking;
            else if (sourceType == XBS.SourceType.MobileBanking)
                loginInfo.PlatformType = PlatformType.MobileBanking;
            loginInfo.IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();

            if (!string.IsNullOrEmpty(request.OTP))
                loginInfo.OTP = request.OTP;


            //Օպերացիոն համակարգ
            loginInfo.AdditionalDetails = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(Request.HttpContext.Request.Headers["Dev"]))
            {
                loginInfo.AdditionalDetails.Add("OS", Request.HttpContext.Request.Headers["Dev"]);
            }
            else
            {
                loginInfo.AdditionalDetails.Add("OS", "0");
            }

            LoginResult loginResult = _cache.Get<string, LoginResult>(sessionId + "_LoginResult");
            if (loginResult != null && loginResult.IsAuthorized)
            {
                var aoUserData = _xbSecurity.AuthorizeUserByToken(loginInfo, loginResult, lang, request.HostName);
                if (aoUserData.AuthorizationResult.IsAuthorized)
                {
                    response.ResultCode = ResultCodes.normal;
                    result.SessionId = aoUserData.SessionID;
                    result.PasswordChangeRequirement = aoUserData.ChangeRequirement;
                    result.UserPermission = aoUserData.Permission;
                    result.IsLastConfirmer = aoUserData.IsLastConfirmer;
                    result.SecondConfirmRequirement = aoUserData.SecondConfirm;
                    result.UserName = aoUserData.UserName;
                    var customerMainData = _xbService.GetCustomerMainData(ulong.Parse(aoUserData.CustomerNumber));
                    result.IsEmployee = _xbService.IsEmployee(ulong.Parse(aoUserData.CustomerNumber));
                    _cache.Set(sessionId + "_IsEmployee", result.IsEmployee);
                    result.FullName = customerMainData.CustomerDescription;
                    result.FullNameEnglish = customerMainData.CustomerDescriptionEng;
                    result.CustomerType = (XBS.CustomerTypes)customerMainData.CustomerType;

                    if (!Convert.ToBoolean(_config["TestVersion"]) && !(result.IsEmployee || loginInfo.UserName == "AcbaA1995"
                    || loginInfo.UserName == "aghazaryan1994"
                    || loginInfo.UserName == "ani-arshakyan"
                    || loginInfo.UserName == "anna5825"
                    || loginInfo.UserName == "anna95"
                    || loginInfo.UserName == "aram280882"
                    || loginInfo.UserName == "aranar"
                    || loginInfo.UserName == "Arman"
                    || loginInfo.UserName == "Arus123"
                    || loginInfo.UserName == "brutyand"
                    || loginInfo.UserName == "DAvinchi123"
                    || loginInfo.UserName == "em805ser16"
                    || loginInfo.UserName == "Ernrk96"
                    || loginInfo.UserName == "gevorgyanara1"
                    || loginInfo.UserName == "Irina27"
                    || loginInfo.UserName == "irinak"
                    || loginInfo.UserName == "Ishkhan"
                    || loginInfo.UserName == "lightning3"
                    || loginInfo.UserName == "mariannas"
                    || loginInfo.UserName == "maroyanlaura"
                    || loginInfo.UserName == "s_tigran"
                    || loginInfo.UserName == "shushanikb"
                    || loginInfo.UserName == "s_tigran"
                    || loginInfo.UserName == "sipan91"
                    || loginInfo.UserName == "Yevginemuradyan"
                    || loginInfo.UserName == "helix"
                    || loginInfo.UserName == "babkenmakaryan"
                    || loginInfo.UserName == "DVahagn"
                    || loginInfo.UserName == "Atlanta"
                    || loginInfo.UserName == "asatryantigran"
                    || loginInfo.UserName == "mardumyan"
                    || loginInfo.UserName == "vyerkanian"
                    || loginInfo.UserName == "manushak84"
                    || loginInfo.UserName == "inga-helix"
                    || loginInfo.UserName == "helix_inga"
                    || loginInfo.UserName == "aram-helix"
                    || loginInfo.UserName == "seynyan"
                    || loginInfo.UserName == "artezia1"
                    || loginInfo.UserName == "artezia4"
                    || loginInfo.UserName == "sevakn"
                    || loginInfo.UserName == "ars1313"
                    || loginInfo.UserName == "eliza"
                    || loginInfo.UserName == "HovhannisyanSuren"
                    || loginInfo.UserName == "artyomg"
                    || loginInfo.UserName == "arminetumanyan"
                    || loginInfo.UserName == "lusine_k"
                    || loginInfo.UserName == "Geghamva"
                    || loginInfo.UserName == "goharg"
                    || loginInfo.UserName == "taguhi"
                    || loginInfo.UserName == "marinka"
                    || loginInfo.UserName == "gy.bakhshyan"
                    || loginInfo.UserName == "S19962504"
                    || loginInfo.UserName == "lilithov"
                    || loginInfo.UserName == "GarMar"
                    || loginInfo.UserName == "aylefO"
                    || loginInfo.UserName == "NarekMMMM"
                    || loginInfo.UserName == "956095St"
                    || loginInfo.UserName == "Mikhail"
                    || loginInfo.UserName == "DavitM"
                    || loginInfo.UserName == "Ripa333Ripa"
                    || loginInfo.UserName == "KGogA"
                    || loginInfo.UserName == "arman.mur"
                    || loginInfo.UserName == "hakob13246"
                    || loginInfo.UserName == "samvel.sairitsyan"
                    || loginInfo.UserName == "miliart"
                    || loginInfo.UserName == "aniyer"
                    || loginInfo.UserName == "Hush.Grig"
                    || loginInfo.UserName == "AniPog"
                    || loginInfo.UserName == "DianaA"
                    || loginInfo.UserName == "lil.ghevondyan"
                    || loginInfo.UserName == "AstghikBur"
                    || loginInfo.UserName == "Anuta"
                    || loginInfo.UserName == "sairitsyan"
                    || loginInfo.UserName == "anigrig"
                    || loginInfo.UserName == "Davit555"
                    || loginInfo.UserName == "Dvahagn"
                    || loginInfo.UserName == "tigranga1"
                    || loginInfo.UserName == "arus123"
                    || loginInfo.UserName == "Sergeyhak"
                    || loginInfo.UserName == "i.arto"
                    || loginInfo.UserName == "i.arto1"
                    || loginInfo.UserName == "kartshikyan"
                    ))
                    {
                        response.ResultCode = ResultCodes.validationError;
                        response.Description = "Դուք չունեք համակարգ մուտք գործելու հասանելիություն։";
                    }
                    else
                    {
                        response.Result = result;
                    }

                    try
                    {
                        _xbService.SendReminderNote(ulong.Parse(aoUserData.CustomerNumber));
                    }
                    catch (Exception)
                    {
                        ///////
                        //response.ResultCode = ResultCodes.failed;
                        //response.Description = ex.Message;
                    }
                }
                else
                {
                    response.ResultCode = ResultCodes.validationError;
                    response.Description = aoUserData.AuthorizationResult.DescriptionAM + " " + aoUserData.AuthorizationResult.Description;
                }
            }
            else
            {
                response.ResultCode = ResultCodes.notAuthorized;
            }
            return ResponseExtensions.ToHttpResponse(response);
        }

        /// <summary>
        /// Ստուգում է վավերականությունը
        /// </summary>
        /// <returns></returns>
        [HttpPost("CheckAuthorization")]
        [TypeFilter(typeof(AuthorizationFilter))]
        public IActionResult CheckAuthorization()
        {
            return ResponseExtensions.ToHttpResponse(new SingleResponse<bool> { Result = true, ResultCode = ResultCodes.normal });
        }

        /// <summary>
        /// LogOut
        /// </summary>
        /// <returns></returns>
        [HttpPost("LogOut")]
        public IActionResult LogOut()
        {
            string sessionId = "";

            if (!string.IsNullOrEmpty(Request.Headers["SessionId"]))
                sessionId = Request.Headers["SessionId"];

            _xbSecurity.LogOut(sessionId);

            return ResponseExtensions.ToHttpResponse(new SingleResponse<bool> { Result = true, ResultCode = ResultCodes.normal });
        }

    }
}