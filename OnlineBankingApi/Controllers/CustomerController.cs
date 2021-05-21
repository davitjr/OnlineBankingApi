using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using XBS;
using XBSecurity;
using static OnlineBankingApi.Enumerations;
using ActionResult = XBS.ActionResult;
using ResponseExtensions = OnlineBankingApi.Models.ResponseExtensions;
using utils = OnlineBankingLibrary.Utilities.Utils;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class CustomerController : ControllerBase
    {
        private readonly XBService _xbService;
        private readonly ACBAOperationService _acbaOperationService;
        private readonly CacheHelper _cacheHelper;
        private readonly XBSecurityService _xbSecurityService;
        public CustomerController(XBService xbService, ACBAOperationService acbaOperationService, CacheHelper cacheHelper, XBSecurityService xBSecurityService)
        {
            _xbService = xbService;
            _acbaOperationService = acbaOperationService;
            _cacheHelper = cacheHelper;
            _xbSecurityService = xBSecurityService;
        }

        /// <summary>
        /// Վերադարձնում է երրորդ անձանց
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetThirdPersons")]
        public IActionResult GetThirdPersons()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<KeyValuePair<ulong, string>>> response = new SingleResponse<List<KeyValuePair<ulong, string>>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetThirdPersons();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի տվյալ պահին տվյալ արժույթով հասանելի գումարը
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        [HttpPost("GetCustomerAvailableAmount")]
        public IActionResult GetCustomerAvailableAmount(CurrencyRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<double> response = new SingleResponse<double>();
                response.Result = _xbService.GetCustomerAvailableAmount(request.Currency);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հաճախորդի հեռախոսահամարների ստացում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        [HttpPost("GetPhoneNumbers")]
        public IActionResult GetPhoneNumbers(CustomerNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<CustomerPhone>> response = new SingleResponse<List<CustomerPhone>>();
                response.Result = _xbService.GetPhoneNumbers(request.CustomerNumber);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հաճախորդի Էլ․փոստերի ստացում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        [HttpPost("GetEmails")]
        public IActionResult GetEmails(CustomerNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<CustomerEmail>> response = new SingleResponse<List<CustomerEmail>>();
                response.Result = _xbService.GetEmails(request.CustomerNumber);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հաճախորդի պրոֆիլի նկարի պահպանում
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SaveOnlineUserPhoto")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveOnlineUserPhoto(SaveOnlineUserPhotoRequest request)
        {
            if (ModelState.IsValid)
            {
                Response response = new Response();
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                ulong onlineUserId = (ulong)authorizedCustomer.UserId;
                byte[] photo = System.Convert.FromBase64String(request.Photo);
                string extension = request.Extension;
                _acbaOperationService.SaveOnlineUserPhoto(onlineUserId, photo, extension);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հաճախորդի պրոֆիլի նկարի հեռացում
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("DeleteOnlineUserPhoto")]
        public IActionResult DeleteOnlineUserPhoto()
        {
            if (ModelState.IsValid)
            {
                Response response = new Response();
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                ulong onlineUserId = (ulong)authorizedCustomer.UserId;
                _acbaOperationService.DeleteOnlineUserPhoto(onlineUserId);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հաճախորդի պրոֆիլի նկարի տվյալներ
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetOnlineUserPhoto")]
        public IActionResult GetOnlineUserPhoto()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                ulong onlineUserId = (ulong)authorizedCustomer.UserId;
                byte[] photo = _acbaOperationService.GetOnlineUserPhoto(onlineUserId);
                if (photo != null)
                {
                    response.Result = Convert.ToBase64String(photo);
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
        /// Վերադարձնում է հաճախորդի տվյալները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetUserInfo")]
        public IActionResult GetUserInfo()
        {
            SingleResponse<AuthorizationResponse> response = new SingleResponse<AuthorizationResponse>();

            string sessionId = "";
            byte language = 0;

            //Սեսիայի նունականացման համար
            if (!string.IsNullOrEmpty(HttpContext.Request.Headers["SessionId"]))
                sessionId = HttpContext.Request.Headers["SessionId"];

            //Լեզու
            if (!string.IsNullOrEmpty(HttpContext.Request.Headers["language"]))
                byte.TryParse(HttpContext.Request.Headers["language"], out language);

            OnlineBankingUser onlineUserData = _xbSecurityService.CheckAuthorization(sessionId, language);

            if (onlineUserData.AuthorizationResult.IsAuthorized)
            {
                CustomerMainData customerMainData = _xbService.GetCustomerMainData(ulong.Parse(onlineUserData.CustomerNumber));

                if(customerMainData.CustomerType != 6)
                {
                    var str = customerMainData.CustomerDescription;
                    var symbols = new string[] { "«", "»"};
                    foreach (var s in symbols)
                    {
                        str = str.Replace(s, "");
                    }

                    customerMainData.CustomerDescription = str;
                }

                response.Result = new AuthorizationResponse();
                response.Result.SessionId = onlineUserData.SessionID;
                response.Result.PasswordChangeRequirement = onlineUserData.ChangeRequirement;
                response.Result.UserPermission = onlineUserData.Permission;
                response.Result.FullName = customerMainData.CustomerDescription;
                response.Result.FullNameEnglish = customerMainData.CustomerDescriptionEng;
                response.Result.IsLastConfirmer = onlineUserData.IsLastConfirmer;
                response.Result.CustomerType = (XBS.CustomerTypes)onlineUserData.TypeOfClient;
                response.Result.SecondConfirmRequirement = onlineUserData.SecondConfirm;
                response.Result.IsEmployee = _xbService.IsEmployee(ulong.Parse(onlineUserData.CustomerNumber));
                response.Result.UserName = onlineUserData.UserName;
                response.ResultCode = ResultCodes.normal;
                try
                {
                    response.Result.TotalAvailableBalance = _xbService.GetUserTotalAvailableBalance(onlineUserData.UserID, ulong.Parse(onlineUserData.CustomerNumber));
                }
                catch (Exception)
                {
                    response.Result.TotalAvailableBalance = null;
                }

            }
            return ResponseExtensions.ToHttpResponse(response);
        }


        /// <summary>
        /// ACBA Digital տիրույթում ընդհանուր հասանելի մնացորդի կարգավորումների ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCustomerAccountRestConfig")]
        public IActionResult GetCustomerAccountRestConfig()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<DigitalAccountRestConfigurations> response = new SingleResponse<DigitalAccountRestConfigurations>();
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                int onlineUserId = authorizedCustomer.UserId;
                response.Result = _xbService.GetCustomerAccountRestConfig(onlineUserId);
                response.Result.Configurations.Select(x => x.CustomerNumber = authorizedCustomer.CustomerNumber);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// ACBA Digital տիրույթում ընդհանուր հասանելի մնացորդի Կարգավորումների փոփոխում
        /// </summary>
        /// <returns></returns>
        [HttpPost("UpdateCustomerAccountRestConfig")]
        public IActionResult UpdateCustomerAccountRestConfig(RestConfigRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
           
                request.ConfigurationItems.ForEach(item => {
                    item.CustomerNumber = authorizedCustomer.CustomerNumber;
                    item.DigitalUserId = authorizedCustomer.UserId;
                    item.RegistrationDate = DateTime.Now;
                    item.ConfigurationTypeId = 2;
                });

               ActionResult saveResult = _xbService.UpdateCustomerAccountRestConfig(request.ConfigurationItems);
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
        /// ACBA Digital տիրույթում ընդհանուր հասանելի մնացորդի Կարգավորումների նախնական վիճակի բերում
        /// </summary>
        /// <returns></returns>
        [HttpPost("ResetCustomerAccountRestConfig")]
        public IActionResult ResetCustomerAccountRestConfig()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                int onlineUserId = authorizedCustomer.UserId;
                ActionResult saveResult = _xbService.ResetCustomerAccountRestConfig(onlineUserId);
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

    }
}