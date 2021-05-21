using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Resources;
using OnlineBankingLibrary.Models;
using OnlineBankingLibrary.Utilities;
using System.Linq;
using XBS;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Filters
{
    public class ValidateOtpFilter : ActionFilterAttribute
    {
        private readonly CacheHelper _cacheHelper;
        private readonly CacheManager _cache;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public ValidateOtpFilter(CacheHelper cacheHelper, CacheManager cache, IStringLocalizer<SharedResource> localizer)
        {
            _cacheHelper = cacheHelper;
            _cache = cache;
            _localizer = localizer;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.ActionArguments["request"] as dynamic;
            Response response = new Response
            {
                ResultCode = ResultCodes.validationError,
                Description = _localizer["Մուտքագրված տվյալները սխալ են կամ ոչ լիարժեք։"]
            };
            CustomerTokenInfo customerTokenInfo = _cacheHelper.GetCustomerTokenInfo();

            if (!string.IsNullOrEmpty(context.HttpContext.Request.Headers["SessionId"]) && customerTokenInfo != null)
            {
                string sessionId = context.HttpContext.Request.Headers["SessionId"];
                AuthorizedCustomer authorizedCustomer = new AuthorizedCustomer()
                {
                    CustomerNumber = customerTokenInfo.CustomerNumber,
                    UserId = customerTokenInfo.UserId,
                    SessionID = sessionId,
                    UserName = customerTokenInfo.UserName,
                };
                if (customerTokenInfo.Checked == true && ((string)context.RouteData.Values["action"] == "SaveAndApproveTokenReplacementOrder" || (string)context.RouteData.Values["action"] == "SaveAndApproveTokenDeactivationOrder"))
                {
                    _cache.Set(sessionId + "_authorizedCustomer", authorizedCustomer);
                    _cache.Set(sessionId + "_Language", customerTokenInfo.Language);
                    _cache.Set(sessionId + "_SourceType", customerTokenInfo.SourceType);
                    _cache.Set(sessionId + "_ClientIp", context.HttpContext.Connection.RemoteIpAddress.ToString());
                }
                else
                {
                    if (!string.IsNullOrEmpty(request.Otp))
                    {

                        string otp = request.Otp;
                        if (!string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(otp))
                        {
                            if (customerTokenInfo == null || (customerTokenInfo != null && otp != customerTokenInfo?.Otp))
                            {
                                context.Result = ResponseExtensions.ToHttpResponse(response);
                            }
                            else
                            {
                                customerTokenInfo.Checked = true;
                                _cache.Set(sessionId + "_authorizedCustomer", authorizedCustomer);
                                _cache.Set(sessionId + "_Language", customerTokenInfo.Language);
                                _cache.Set(sessionId + "_SourceType", customerTokenInfo.SourceType);
                                _cache.Set(sessionId + "_ClientIp", context.HttpContext.Connection.RemoteIpAddress.ToString());
                            }
                        }
                        else
                        {
                            context.Result = ResponseExtensions.ToHttpResponse(response);
                        }
                    }
                    else
                    {
                        context.Result = ResponseExtensions.ToHttpResponse(response);
                    }
                }
            }
            else
            {
                context.Result = ResponseExtensions.ToHttpResponse(response);
            }
        }
    }
}
