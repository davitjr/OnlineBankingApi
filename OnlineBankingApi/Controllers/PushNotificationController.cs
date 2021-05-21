using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingLibrary.Models;
using OnlineBankingLibrary.Services;
using XBSecurity;
using static OnlineBankingApi.Enumerations;
using PushNotificationService;
using OnlineBankingApi.Utilities;
using OnlineBankingApi.Models.Requests;
using OnlineBankingLibrary.Utilities;
using XBS;
using ACBAServiceReference;

namespace OnlineBankingApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class PushNotificationController : ControllerBase
    {
        private readonly XBSecurityPushNotificationService _xbSecurityPushNotificationService;
        private readonly CacheHelper _cacheHelper;
        private readonly ACBAOperationService _aCBAOperationService;
        public PushNotificationController(XBSecurityPushNotificationService xbSecurityPushNotificationService, CacheHelper cacheHelper, ACBAOperationService aCBAOperationService)
        {
            _xbSecurityPushNotificationService = xbSecurityPushNotificationService;
            _cacheHelper = cacheHelper;
            _aCBAOperationService = aCBAOperationService;
        }

        /// <summary>
        /// Պահպանում է Firebase տոկենը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SaveNotificationToken")]
        public IActionResult SaveNotificationToken(NotificationTokenRequest request)
        {
            var response = new SingleResponse<long>() { ResultCode = ResultCodes.normal };
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            if (request?.Token != null)
            {
                request.Token.CustomerType = (byte)authorizedCustomer.TypeOfClient;
                request.Token.CustomerNumber = authorizedCustomer.CustomerNumber;
                request.Token.UserId = authorizedCustomer.UserId;
                PushNotificationService.ActionResult saveResult = _xbSecurityPushNotificationService.SaveNotificationToken(request.Token);
                response.ResultCode = ResultCodeFormatter.FromPushNotificationService(saveResult.ActionResultCode);
            }
            else
            {
                response.ResultCode = ResultCodes.failed;
            }
            return ResponseExtensions.ToHttpResponse(response);
        }
    }
}
