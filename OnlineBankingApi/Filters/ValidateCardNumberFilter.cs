using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Resources;
using OnlineBankingLibrary.Services;
using System.Linq;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Filters
{
    public class ValidateCardNumberFilter : ActionFilterAttribute
    {
        private readonly XBService _xbService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public ValidateCardNumberFilter(XBService xbService, IStringLocalizer<SharedResource> localizer)
        {
            _xbService = xbService;
            _localizer = localizer;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {

            if (context.ActionArguments.Count != 0)
            {
                if (context.ActionArguments.Values.First() is CardNumberRequest)
                {
                    CardNumberRequest cardNumber = context.ActionArguments.Values.First() as CardNumberRequest;
                    if (!_xbService.ValidateCardNumber(cardNumber.CardNumber))
                    {
                        Response response = new Response
                        {
                            ResultCode = ResultCodes.validationError,
                            Description = _localizer["Քարտի համարը սխալ է։"]
                        };
                        context.Result = ResponseExtensions.ToHttpResponse(response);
                    }
                }            
            }
        }
    }
}
