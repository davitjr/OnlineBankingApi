using Microsoft.AspNetCore.Mvc.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using System.Linq;
using XBS;
using XBSecurity;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Filters
{
    public class SeconfConfirmationFilter: ActionFilterAttribute
    {
        private readonly OnlineBankingLibrary.Services.XBSecurityService _xbSecurity;
        private readonly CacheHelper _cacheHelper;

        public SeconfConfirmationFilter(OnlineBankingLibrary.Services.XBSecurityService xbSecurity, CacheHelper cacheHelper)
        {
            _xbSecurity = xbSecurity;
            _cacheHelper = cacheHelper;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();

            if(_cacheHelper.GetSourceType() == XBS.SourceType.AcbaOnline && authorizedCustomer.SecondConfirm == 1)
            {
                bool isValid;
                ApproveIdRequest request = null;
                ProductIdApproveRequest requestProductId = null;
                OrderRejectionRequest rejectionRequest = null;
                RemovalOrderRequest removalOrderRequest = null;
                string OTP = "";

                foreach (var argument in context.ActionArguments.Values.Where(v => v is ApproveIdRequest))
                {                  
                    request = argument as ApproveIdRequest;
                    OTP = request.OTP;
                    break;
                }

                foreach (var argument in context.ActionArguments.Values.Where(v => v is OrderRejectionRequest))
                {
                    rejectionRequest = argument as OrderRejectionRequest;
                    OTP = rejectionRequest.OTP;
                    break;
                }

                foreach (var argument in context.ActionArguments.Values.Where(v => v is RemovalOrderRequest))
                {
                    removalOrderRequest = argument as RemovalOrderRequest;
                    OTP = removalOrderRequest.OTP;
                    break;
                }

                foreach (var argument in context.ActionArguments.Values.Where(v => v is ProductIdApproveRequest))
                {
                    requestProductId = argument as ProductIdApproveRequest;
                    OTP = request.OTP;
                    break;
                }

                isValid = _xbSecurity.ValidateOTP(context.HttpContext.Request.Headers["SessionId"], OTP, _cacheHelper.GetClientIp(), _cacheHelper.GetLanguage());

                if (!isValid)
                {
                    Response response = new Response();
                    response.ResultCode = ResultCodes.validationError;
                    response.Description = (Languages)_cacheHelper.GetLanguage() == Languages.hy ? "Սխալ թվային կոդ։" : "Incorrect OTP code.";

                    context.Result = ResponseExtensions.ToHttpResponse(response);
                }
            }
           
        }
    }
}
