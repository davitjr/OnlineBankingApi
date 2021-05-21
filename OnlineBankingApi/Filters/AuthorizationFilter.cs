using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using OnlineBankingApi.Models;
using static OnlineBankingApi.Enumerations;
using ResponseExtensions = OnlineBankingApi.Models.ResponseExtensions;
using OnlineBankingUser = XBSecurity.OnlineBankingUser;
using OnlineBankingLibrary.Services;
using XBS;
using OnlineBankingLibrary.Utilities;
using Microsoft.Extensions.Configuration;

namespace OnlineBankingApi.Filters
{
    public class AuthorizationFilter : Attribute, IAuthorizationFilter
    {
        private readonly XBSecurityService _xbSecurityService;
        private readonly XBService _xbService;
        private readonly CacheManager _cache;
        private readonly IConfiguration _config;

        public AuthorizationFilter(XBSecurityService xbSecurityService, XBService xbService, CacheManager cache,IConfiguration config)
        {
            _xbSecurityService = xbSecurityService;
            _xbService = xbService;
            _cache = cache;
            _config = config;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            Response response = new Response();
            AuthorizedCustomer authorizedCustomer = new AuthorizedCustomer();
            string sessionId = "";
            byte language = 0;
            string ipAddress;
            SourceType sourceType = SourceType.NotSpecified;

            if (context.Filters.Any(item => item is IAllowAnonymousFilter))
            {
                return;
            }

            //Սեսիայի նունականացման համար
            if (!string.IsNullOrEmpty(context.HttpContext.Request.Headers["SessionId"]))
                sessionId = context.HttpContext.Request.Headers["SessionId"];

            //Լեզու
            if (!string.IsNullOrEmpty(context.HttpContext.Request.Headers["language"]))
                byte.TryParse(context.HttpContext.Request.Headers["language"], out language);

            //Տվյալների մուտքագրման աղբյուր
            if (!string.IsNullOrEmpty(context.HttpContext.Request.Headers["SourceType"]))
                SourceType.TryParse(context.HttpContext.Request.Headers["SourceType"], out sourceType);

            //IP հասցե
            ipAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();

            if (sessionId == "ba0f312d-8487-445e-aee2-d5877ac1d4de" && Convert.ToBoolean(_config["TestVersion"]))
            {
                authorizedCustomer = _xbService.GetTestMobileBankingUser();
                authorizedCustomer.CustomerNumber = Convert.ToUInt64(context.HttpContext.Request.Headers["customerNumber"]);
                authorizedCustomer.IsEmployee = _xbService.IsEmployee(authorizedCustomer.CustomerNumber);
                if (authorizedCustomer.CustomerNumber == 0)
                {
                    authorizedCustomer.CustomerNumber = 1111;
                }
                
                _cache.Set(sessionId + "_ClientIp", "169.169.169.166");
                _cache.Set(sessionId + "_Language", language);
                _cache.Set(sessionId + "_authorizedCustomer", authorizedCustomer);
                _cache.Set(sessionId + "_SourceType", sourceType);
                authorizedCustomer.UserId = 55;
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    _cache.Set(sessionId + "_userProductPermission", _xbService.GetUserProductsPermissions(authorizedCustomer.UserName));
                }
            }
            else
            {
                if (sessionId != "")
                {
                    OnlineBankingUser onlineUserData = new OnlineBankingUser();
                    onlineUserData = _xbSecurityService.CheckAuthorization(sessionId, language);

                    if (onlineUserData.AuthorizationResult.IsAuthorized)
                    {
                        authorizedCustomer.CustomerNumber = ulong.Parse(onlineUserData.CustomerNumber);
                        authorizedCustomer.UserName = onlineUserData.UserName;
                        authorizedCustomer.UserId = onlineUserData.UserID;
                        authorizedCustomer.DailyTransactionsLimit = onlineUserData.DailyTransactionsLimit;
                        authorizedCustomer.OneTransactionLimit = onlineUserData.OneTransactionLimit;
                        _cache.Set(sessionId + "_ClientIp", ipAddress);
                        authorizedCustomer.ApprovementScheme = short.Parse(onlineUserData.ApprovementScheme.ToString());
                        authorizedCustomer.LimitedAccess = onlineUserData.LimitedAccess;
                        authorizedCustomer.TypeOfClient = onlineUserData.TypeOfClient;
                        authorizedCustomer.Permission = onlineUserData.Permission;
                        authorizedCustomer.SecondConfirm = onlineUserData.SecondConfirm;
                        authorizedCustomer.IsLastConfirmer = onlineUserData.IsLastConfirmer;
                        authorizedCustomer.BranchCode = onlineUserData.BranchCode;
                        authorizedCustomer.IsEmployee = _xbService.IsEmployee(authorizedCustomer.CustomerNumber);

                        _cache.Set(sessionId + "_Language", language);
                        if (authorizedCustomer.LimitedAccess != 0)
                        {
                            List<HBProductPermission> _userProductPermission = _xbService.GetUserProductsPermissions(authorizedCustomer.UserName);
                            _cache.Set(sessionId + "_userProductPermission", _xbService.GetUserProductsPermissions(authorizedCustomer.UserName));
                        }
                        _cache.Set(sessionId + "_authorizedCustomer", authorizedCustomer);
                        _cache.Set(sessionId + "_SourceType", sourceType);
                    }
                    else
                    {
                        response.ResultCode = ResultCodes.notAuthorized;
                        context.Result = ResponseExtensions.ToHttpResponse(response);
                    }

                }
                else
                {
                    response.ResultCode = ResultCodes.notAuthorized;
                    context.Result = ResponseExtensions.ToHttpResponse(response);
                }
            }
        }
    }
}
