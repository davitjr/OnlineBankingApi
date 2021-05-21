using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Resources;
using OnlineBankingLibrary.Services;
using System.Linq;
using XBS;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Filters
{
    public class ValidateAccountNumberFilter : ActionFilterAttribute
    {
        private readonly XBService _xbService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ValidateAccountNumberFilter(XBService xbService, IStringLocalizer<SharedResource> localizer)
        {
            _xbService = xbService;
            _localizer = localizer;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.Count != 0)
            {
                AccountNumberRequest request = null;

                foreach (var argument in context.ActionArguments.Values.Where(v => v is AccountNumberRequest))
                {
                    request = argument as AccountNumberRequest;
                    break;
                }

                if (!_xbService.ValidateAccountNumber(request.AccountNumber))
                {
                    Response response = new Response
                    {
                        ResultCode = ResultCodes.validationError,
                        Description = _localizer["Հաշվեհամարը սխալ է։"]
                    };
                    context.Result = ResponseExtensions.ToHttpResponse(response);
                } 
            }
        }
    }
}
